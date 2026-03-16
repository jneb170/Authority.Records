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
    /// Parses a Google Maps formatted_address string into individual address components.
    /// Handles the standard US format: "1370 S Rigsbee Dr, Plano, TX 75074, USA"
    ///   → streetNumber="1370", route="S Rigsbee Dr", city="Plano",
    ///     state="TX", zip="75074", country="US"
    /// Also handles suite/apt suffixes on the street part:
    ///   "101 E Park Blvd Suite 600, Plano, TX 75074, USA"
    ///   → route="E Park Blvd", aptSuite="Suite 600"
    /// </summary>
    private static (string? streetNumber, string? route, string? aptSuite, string? city,
                    string? state, string? zip, string? country)
        ParseFormattedAddress(string formatted)
    {
        // Split on ", " — the standard separator Google Maps uses between address parts.
        var parts = formatted.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return (null, null, null, null, null, null, null);

        // ── Street (first part) ──────────────────────────────────────────────
        // "1370 S Rigsbee Dr"         → streetNumber="1370", route="S Rigsbee Dr"
        // "101 E Park Blvd Suite 600" → streetNumber="101",  route="E Park Blvd", aptSuite="Suite 600"
        // "Main Street Clinic"        → streetNumber=null,   route="Main Street Clinic"
        string? streetNumber = null;
        string? route        = null;
        string? aptSuite     = null;

        var streetTokens = parts[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int routeEnd = streetTokens.Length; // exclusive index where route ends

        // Detect suite/apt keywords and split there (start at 0 to catch
        // edge cases like "Suite 600, Dallas, TX" with no street prefix).
        for (int i = 0; i < streetTokens.Length; i++)
        {
            if (SuiteKeywords.Contains(streetTokens[i], StringComparer.OrdinalIgnoreCase))
            {
                aptSuite = string.Join(' ', streetTokens[i..]);
                routeEnd = i;
                break;
            }
        }

        var routeTokens = streetTokens[..routeEnd];
        if (routeTokens.Length >= 2 && int.TryParse(routeTokens[0], out _))
        {
            streetNumber = routeTokens[0];
            route        = string.Join(' ', routeTokens[1..]);
        }
        else if (routeTokens.Length > 0)
        {
            route = string.Join(' ', routeTokens);
        }

        // ── Remaining parts (work from the end) ──────────────────────────────
        // Typical layout (4 parts): street | city | "STATE ZIP" | country
        // 3 parts: street | city | "STATE ZIP"   (country omitted)
        // 5+ parts: extra neighbourhood/suite segment before city — city is ^3
        string? city    = null;
        string? state   = null;
        string? zip     = null;
        string? country = null;

        if (parts.Length >= 4)
        {
            country = NormalizeCountryCode(parts[^1]);
            ParseStateZip(parts[^2], out state, out zip);
            city = parts[^3];
        }
        else if (parts.Length == 3)
        {
            ParseStateZip(parts[^1], out state, out zip);
            city = parts[^2];
        }
        else if (parts.Length == 2)
        {
            city = parts[1];
        }

        return (streetNumber, route, aptSuite, city, state, zip, country);
    }

    // Suite/apt keywords that may appear inline in the street part of a formatted address.
    private static readonly HashSet<string> SuiteKeywords =
    [
        "Suite", "Ste", "Apt", "Apt.", "Unit", "Fl", "Floor",
        "Bldg", "Building", "Rm", "Room", "#"
    ];

    private static void ParseStateZip(string part, out string? state, out string? zip)
    {
        // "TX 75074"  →  state="TX", zip="75074"
        // "TX"        →  state="TX", zip=null
        var t = part.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        state = t.Length > 0 ? t[0] : null;
        zip   = t.Length > 1 ? t[1] : null;
    }

    private static string? NormalizeCountryCode(string? part)
    {
        if (string.IsNullOrWhiteSpace(part)) return null;
        var t = part.Trim();
        if (t.Equals("USA", StringComparison.OrdinalIgnoreCase)
         || t.Contains("United States", StringComparison.OrdinalIgnoreCase))
            return "US";
        if (t.Contains("Canada",  StringComparison.OrdinalIgnoreCase)) return "CA";
        if (t.Contains("Mexico",  StringComparison.OrdinalIgnoreCase)) return "MX";
        // Already a 2-letter code (e.g., from address_components short_name)
        if (t.Length == 2) return t.ToUpperInvariant();
        return null;
    }

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
