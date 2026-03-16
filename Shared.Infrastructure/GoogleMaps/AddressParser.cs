namespace Shared.Infrastructure.GoogleMaps;

/// <summary>
/// Pure-static helpers for parsing Google Maps formatted address strings into
/// discrete address fields.  Extracted from GoogleMapsPlacesClient for testability.
/// </summary>
internal static class AddressParser
{
    // Suite/apt keywords that may appear inline in the street part of a formatted address.
    internal static readonly HashSet<string> SuiteKeywords =
    [
        "Suite", "Ste", "Apt", "Apt.", "Unit", "Fl", "Floor",
        "Bldg", "Building", "Rm", "Room", "#"
    ];

    /// <summary>
    /// Parses a Google Maps formatted_address string into individual address components.
    /// Handles the standard US format: "1370 S Rigsbee Dr, Plano, TX 75074, USA"
    ///   → streetNumber="1370", route="S Rigsbee Dr", city="Plano",
    ///     state="TX", zip="75074", country="US"
    /// Also handles suite/apt suffixes on the street part:
    ///   "101 E Park Blvd Suite 600, Plano, TX 75074, USA"
    ///   → route="E Park Blvd", aptSuite="Suite 600"
    /// </summary>
    internal static (string? streetNumber, string? route, string? aptSuite, string? city,
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

    internal static void ParseStateZip(string part, out string? state, out string? zip)
    {
        // "TX 75074"  →  state="TX", zip="75074"
        // "TX"        →  state="TX", zip=null
        var t = part.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        state = t.Length > 0 ? t[0] : null;
        zip   = t.Length > 1 ? t[1] : null;
    }

    internal static string? NormalizeCountryCode(string? part)
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
}
