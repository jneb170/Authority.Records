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
    private const string BaseUrl      = "https://maps.googleapis.com/maps/api/place/textsearch/json";
    private const int    PageSize     = 20;
    private const int    MaxPages     = 3;
    private const int    PageDelayMs  = 2000;

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
            if (page > 0 && nextPageToken is not null)
                await Task.Delay(PageDelayMs, cancellationToken);

            var url = BuildUrl(keyword, apiKey, nextPageToken);
            var response = await http.GetFromJsonAsync<PlacesTextSearchResponse>(
                url,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                cancellationToken);

            if (response?.Results is null || response.Results.Length == 0)
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
        string? city         = null;
        string? state        = null;
        string? country      = null;
        string? zip          = null;

        if (place.AddressComponents is not null)
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

        double? lat = place.Geometry?.Location?.Lat;
        double? lng = place.Geometry?.Location?.Lng;

        return new GooglePlaceResult(
            PlaceName:        place.Name ?? string.Empty,
            FormattedAddress: place.FormattedAddress ?? string.Empty,
            StreetNumber:     streetNumber,
            StreetAddress:    route,
            City:             city,
            Zip:              zip,
            StateAbbreviation: state,
            CountryCode:      country,
            Lat:              lat,
            Lng:              lng);
    }

    // ── JSON models ──────────────────────────────────────────────────────────

    private sealed class PlacesTextSearchResponse
    {
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
