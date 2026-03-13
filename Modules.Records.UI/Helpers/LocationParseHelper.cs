using Modules.Records.Application.DTOs;

namespace Modules.Records.UI.Helpers;

/// <summary>
/// Shared helper for parsing Google Maps route strings into Location picklist IDs.
/// Used by both LocationCreate and LocationSearchModal.
/// </summary>
public static class LocationParseHelper
{
    private static readonly Dictionary<string, string> DirectionAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["North"] = "N",  ["South"] = "S",  ["East"] = "E",  ["West"] = "W",
        ["Northeast"] = "NE", ["Northwest"] = "NW",
        ["Southeast"] = "SE", ["Southwest"] = "SW"
    };

    private static readonly Dictionary<string, string> StreetTypeAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Street"] = "St",  ["Avenue"] = "Ave",    ["Boulevard"] = "Blvd", ["Drive"] = "Dr",
        ["Court"]  = "Ct",  ["Lane"]   = "Ln",     ["Place"]     = "Pl",   ["Road"]  = "Rd",
        ["Parkway"] = "Pkwy", ["Highway"] = "Hwy", ["Circle"]   = "Cir",  ["Terrace"] = "Ter",
        ["Trail"]   = "Trl",  ["Way"]    = "Way",  ["Pike"]     = "Pike",  ["Expressway"] = "Expy"
    };

    /// <summary>
    /// Splits a Google Maps route string (e.g. "N Main St" or "East Plano Parkway") into
    /// (preDirectionId, streetName, streetTypeId) using loaded picklist items.
    /// Handles both abbreviation values ("E") and long-form aliases ("East").
    /// </summary>
    public static (Guid? preId, string name, Guid? typeId) ParseRoute(
        string route,
        IReadOnlyList<PicklistItemDto> directions,
        IReadOnlyList<PicklistItemDto> streetTypes)
    {
        static PicklistItemDto? FindMatch(IReadOnlyList<PicklistItemDto> items, string token,
            Dictionary<string, string> aliases)
        {
            var direct = items.FirstOrDefault(i => string.Equals(i.Value, token, StringComparison.OrdinalIgnoreCase));
            if (direct is not null) return direct;
            if (aliases.TryGetValue(token, out var abbr))
                return items.FirstOrDefault(i => string.Equals(i.Value, abbr, StringComparison.OrdinalIgnoreCase));
            return null;
        }

        var tokens = route.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        Guid? preId  = null;
        Guid? typeId = null;

        if (tokens.Count > 1)
        {
            var pre = FindMatch(directions, tokens[0], DirectionAliases);
            if (pre is not null)
            {
                preId = pre.Id;
                tokens.RemoveAt(0);
            }
        }

        if (tokens.Count > 1)
        {
            var type = FindMatch(streetTypes, tokens[^1], StreetTypeAliases);
            if (type is not null)
            {
                typeId = type.Id;
                tokens.RemoveAt(tokens.Count - 1);
            }
        }

        return (preId, string.Join(" ", tokens), typeId);
    }
}
