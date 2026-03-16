namespace Modules.Records.Application.Locations;

/// <summary>
/// Pure-static helpers for tokenising a Google Maps route string into its
/// pre-direction, core street name, post-direction, and street-type components,
/// resolving each to picklist GUIDs where possible.
/// Extracted from GenerateTestLocationsHandler for testability.
/// </summary>
internal static class StreetParser
{
    // Full-name → abbreviated synonyms so Google Maps long-form names resolve to picklist values.
    internal static readonly IReadOnlyDictionary<string, string> StreetTypeSynonyms =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Street"]    = "St",
            ["Avenue"]    = "Ave",
            ["Boulevard"] = "Blvd",
            ["Drive"]     = "Dr",
            ["Court"]     = "Ct",
            ["Lane"]      = "Ln",
            ["Place"]     = "Pl",
            ["Road"]      = "Rd",
            ["Highway"]   = "Hwy",
            ["Parkway"]   = "Pkwy",
            ["Way"]       = "Way",
            ["Circle"]    = "Cir",
            ["Trail"]     = "Trl",
            ["Terrace"]   = "Ter",
        };

    /// <summary>
    /// Splits a Google Maps route string (e.g. "S Rigsbee Dr" or "Oak Ave NW") into its
    /// pre-direction, base street name, post-direction, and street-type components,
    /// resolving each to picklist IDs where possible.
    /// Post-direction is checked BEFORE street type so that e.g. "Oak Ave NW" correctly
    /// strips "NW" as a direction first, then "Ave" as the street type.
    /// </summary>
    internal static (Guid? preDir, string? streetName, Guid? postDir, Guid? streetType)
        Parse(
            string?                  route,
            Dictionary<string, Guid> directionDict,
            Dictionary<string, Guid> streetTypeDict)
    {
        if (string.IsNullOrWhiteSpace(route))
            return (null, null, null, null);

        var tokens = route.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0) return (null, null, null, null);

        int start = 0;
        int end   = tokens.Length - 1;

        // 1. Pre-direction: first token
        Guid? preDir = null;
        if (start <= end && directionDict.TryGetValue(tokens[start], out var preDirId))
        {
            preDir = preDirId;
            start++;
        }

        // 2. Post-direction: last token — must be checked BEFORE street type
        //    so "Oak Ave NW" → postDir=NW, then streetType=Ave (not the reverse).
        Guid? postDir = null;
        if (start < end && directionDict.TryGetValue(tokens[end], out var postDirId))
        {
            postDir = postDirId;
            end--;
        }

        // 3. Street type: now-last token (after post-direction has been removed)
        Guid? streetType = null;
        if (start <= end)
        {
            var lastToken = tokens[end];
            if (streetTypeDict.TryGetValue(lastToken, out var stId))
            {
                streetType = stId;
                end--;
            }
            else if (StreetTypeSynonyms.TryGetValue(lastToken, out var abbr)
                  && streetTypeDict.TryGetValue(abbr, out var stId2))
            {
                streetType = stId2;
                end--;
            }
        }

        // 4. Remaining tokens are the core street name
        var streetName = start <= end
            ? string.Join(' ', tokens[start..(end + 1)])
            : null;

        return (preDir, streetName, postDir, streetType);
    }
}
