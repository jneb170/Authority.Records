using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Modules.Records.Application.Abstractions;

namespace Shared.Infrastructure.GoogleMaps;

/// <summary>
/// Calls the Google Maps Places Text Search API (legacy JSON endpoint) to resolve
/// a keyword into a list of real place results.  Handles up to 3 pages of 20
/// results each (maximum 60 locations per call).
/// </summary>
public sealed class GoogleMapsPlacesClient : IGoogleMapsPlacesClient
{
    private const string BaseUrl        = "https://maps.googleapis.com/maps/api/place/textsearch/json";
    private const int    PageSize       = 20;
    private const int    MaxPages       = 3;
    private const int    PageDelayMs    = 2000;
    private const int    MaxRetries     = 3;
    private const int    RetryBaseDelayMs = 1000;

    private readonly IHttpClientFactory _httpFactory;

    public GoogleMapsPlacesClient(IHttpClientFactory httpFactory)
    {
        _httpFactory = httpFactory;
    }

    public async Task<IReadOnlyList<GooglePlaceResult>> SearchAsync(
        string            keyword,
        int               maxResults,
        string            apiKey,
        CancellationToken cancellationToken = default)
    {
        using var http    = _httpFactory.CreateClient("GoogleMapsPlaces");
        var       results = new List<GooglePlaceResult>(maxResults);
        string?   nextPageToken = null;
        int       page    = 0;

        while (results.Count < maxResults && page < MaxPages)
        {
            // Google requires a delay before every pagination request (not just when
            // a next page token is present) because the token becomes valid after a
            // short server-side delay.
            if (page > 0)
                await Task.Delay(PageDelayMs, cancellationToken);

            var url      = BuildUrl(keyword, apiKey, nextPageToken);
            var response = await FetchWithRetryAsync(http, url, cancellationToken);

            // Check the API-level status before processing results.
            // Google returns HTTP 200 for all responses — error codes live in the body.
            if (!string.IsNullOrWhiteSpace(response.Status)
             && response.Status != "OK"
             && response.Status != "ZERO_RESULTS")
            {
                var detail = response.ErrorMessage ?? response.Status;
                throw new InvalidOperationException(
                    $"Google Places API returned an error: {detail}");
            }

            if (response.Results is null || response.Results.Length == 0)
                break;

            foreach (var place in response.Results)
            {
                if (results.Count >= maxResults) break;
                var parsed = Parse(place);
                if (parsed is not null)
                    results.Add(parsed);
            }

            nextPageToken = response.NextPageToken;
            if (string.IsNullOrWhiteSpace(nextPageToken))
                break;

            page++;
        }

        return results;
    }

    // ── Retry helper ─────────────────────────────────────────────────────────

    /// <summary>
    /// Fetches a single page from the Places API with exponential-backoff retry
    /// for transient HTTP failures (429, 5xx, network errors).
    /// </summary>
    private static async Task<PlacesTextSearchResponse> FetchWithRetryAsync(
        HttpClient        http,
        string            url,
        CancellationToken cancellationToken)
    {
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            try
            {
                var response = await http.GetFromJsonAsync<PlacesTextSearchResponse>(
                    url, jsonOptions, cancellationToken);

                // A 429 or retryable API status comes back as HTTP 200 with a body;
                // return the raw response and let the caller check the Status field.
                return response ?? new PlacesTextSearchResponse();
            }
            catch (HttpRequestException ex) when (attempt < MaxRetries
                && ex.StatusCode is System.Net.HttpStatusCode.TooManyRequests
                                 or System.Net.HttpStatusCode.ServiceUnavailable
                                 or System.Net.HttpStatusCode.GatewayTimeout
                                 or System.Net.HttpStatusCode.InternalServerError)
            {
                var delay = RetryBaseDelayMs * (int)Math.Pow(2, attempt);
                await Task.Delay(delay, cancellationToken);
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested
                                                 && attempt < MaxRetries)
            {
                // Transient timeout — retry with backoff
                var delay = RetryBaseDelayMs * (int)Math.Pow(2, attempt);
                await Task.Delay(delay, cancellationToken);
            }
        }

        // Final attempt — let any exception propagate
        return await http.GetFromJsonAsync<PlacesTextSearchResponse>(
            url, jsonOptions, cancellationToken)
            ?? new PlacesTextSearchResponse();
    }

    // ── URL builder ──────────────────────────────────────────────────────────

    private static string BuildUrl(string keyword, string apiKey, string? pageToken)
    {
        var encoded = Uri.EscapeDataString(keyword);
        var url     = $"{BaseUrl}?query={encoded}&key={Uri.EscapeDataString(apiKey)}";
        if (!string.IsNullOrWhiteSpace(pageToken))
            url += $"&pagetoken={Uri.EscapeDataString(pageToken)}";
        return url;
    }

    // ── Response parsing ─────────────────────────────────────────────────────

    private static GooglePlaceResult? Parse(PlaceResult place)
    {
        if (place is null) return null;

        string? streetNumber = null;
        string? route        = null;
        string? aptSuite     = null;
        string? city         = null;
        string? state        = null;
        string? country      = null;
        string? zip          = null;

        // address_components are only returned by the Place Details and Geocoding
        // APIs, NOT by the Text Search API used here.  When present, use them;
        // otherwise fall back to parsing formatted_address.
        if (place.AddressComponents is { Length: > 0 })
        {
            foreach (var component in place.AddressComponents)
            {
                if (component.Types is null) continue;

                if (component.Types.Contains("street_number"))
                    streetNumber = component.ShortName;
                else if (component.Types.Contains("route"))
                    route = component.LongName;
                else if (component.Types.Contains("locality"))
                    city = component.LongName;
                else if (component.Types.Contains("administrative_area_level_1"))
                    state = component.ShortName;
                else if (component.Types.Contains("country"))
                    country = component.ShortName;
                else if (component.Types.Contains("postal_code"))
                    zip = component.LongName;
            }
        }
        else if (!string.IsNullOrWhiteSpace(place.FormattedAddress))
        {
            (streetNumber, route, aptSuite, city, state, zip, country) =
                ParseFormattedAddress(place.FormattedAddress);
        }

        double? lat = place.Geometry?.Location?.Lat;
        double? lng = place.Geometry?.Location?.Lng;

        return new GooglePlaceResult(
            PlaceName:         place.Name ?? string.Empty,
            FormattedAddress:  place.FormattedAddress ?? string.Empty,
            StreetNumber:      streetNumber,
            StreetAddress:     route,
            AptSuite:          aptSuite,
            City:              city,
            Zip:               zip,
            StateAbbreviation: state,
            CountryCode:       country,
            Lat:               lat,
            Lng:               lng);
    }

    /// <summary>
    /// Parses a Google Maps formatted_address string. Delegates to <see cref="AddressParser"/>.
    /// </summary>
    private static (string? streetNumber, string? route, string? aptSuite, string? city,
                    string? state, string? zip, string? country)
        ParseFormattedAddress(string formatted)
        => AddressParser.ParseFormattedAddress(formatted);

    // Suite/apt keywords — delegates to AddressParser for consistency.
    private static readonly HashSet<string> SuiteKeywords = AddressParser.SuiteKeywords;

    private static void ParseStateZip(string part, out string? state, out string? zip)
        => AddressParser.ParseStateZip(part, out state, out zip);

    private static string? NormalizeCountryCode(string? part)
        => AddressParser.NormalizeCountryCode(part);

    // ── JSON models ──────────────────────────────────────────────────────────

    private sealed class PlacesTextSearchResponse
    {
        public string?        Status        { get; set; }

        [JsonPropertyName("error_message")]
        public string?        ErrorMessage  { get; set; }

        public PlaceResult[]? Results       { get; set; }

        [JsonPropertyName("next_page_token")]
        public string?        NextPageToken { get; set; }
    }

    private sealed class PlaceResult
    {
        public string?             Name             { get; set; }

        [JsonPropertyName("formatted_address")]
        public string?             FormattedAddress { get; set; }

        [JsonPropertyName("address_components")]
        public AddressComponent[]? AddressComponents { get; set; }

        public Geometry?           Geometry         { get; set; }
    }

    private sealed class AddressComponent
    {
        [JsonPropertyName("long_name")]
        public string?   LongName  { get; set; }

        [JsonPropertyName("short_name")]
        public string?   ShortName { get; set; }

        public string[]? Types     { get; set; }
    }

    private sealed class Geometry
    {
        public LatLng? Location { get; set; }
    }

    private sealed class LatLng
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}
