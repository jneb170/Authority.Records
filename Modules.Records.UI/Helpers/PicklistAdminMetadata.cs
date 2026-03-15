using System.Text;
using Modules.Records.Domain.Common;

namespace Modules.Records.UI.Helpers;

public static class PicklistAdminMetadata
{
    public static string GetDisplayName(string type) => type switch
    {
        PicklistTypes.HairColor => "Hair Color",
        PicklistTypes.EyeColor  => "Eye Color",
        PicklistTypes.StreetType => "Street Type",
        _ => SplitPascalCase(type)
    };

    public static string GetDescription(string type) => type switch
    {
        PicklistTypes.ArrestType => "Booking and arrest classification options used on arrest records.",
        PicklistTypes.Court => "Court destinations and filing options used on citations.",
        PicklistTypes.Race => "Master Name Index descriptors for race and ethnicity reporting.",
        PicklistTypes.Sex => "Master Name Index descriptors for sex and gender entry.",
        PicklistTypes.Suffix => "Name suffixes such as JR, SR, II, and professional designators.",
        PicklistTypes.State => "US states and territories used for licenses and address entry.",
        PicklistTypes.HairColor => "Master Name Index descriptors for physical description entry.",
        PicklistTypes.EyeColor => "Master Name Index descriptors for physical description entry.",
        PicklistTypes.Direction => "Pre- and post-direction values for address formatting.",
        PicklistTypes.StreetType => "Street suffix values such as St, Ave, Blvd, and Rd.",
        PicklistTypes.Country => "Country values used on address records and location entry.",
        _ => "Additional configurable dropdown values used across records."
    };

    public static string GetGroupKey(string type) => type switch
    {
        PicklistTypes.Race or PicklistTypes.Sex or PicklistTypes.Suffix or PicklistTypes.HairColor or PicklistTypes.EyeColor or PicklistTypes.State
            => "mni",
        PicklistTypes.Direction or PicklistTypes.StreetType or PicklistTypes.Country
            => "mli",
        PicklistTypes.ArrestType or PicklistTypes.Court
            => "records",
        _ => "other"
    };

    public static string GetGroupName(string groupKey) => groupKey switch
    {
        "records" => "Records and Case Processing",
        "mni" => "Master Name Index",
        "mli" => "Master Location Index",
        _ => "Other Picklists"
    };

    public static string GetGroupDescription(string groupKey) => groupKey switch
    {
        "records" => "Picklists tied directly to arrest, citation, and case workflow.",
        "mni" => "Person-description values reused across names, identifiers, and demographic entry.",
        "mli" => "Address and location values used for street formatting and jurisdictional data.",
        _ => "Additional dropdown sets that do not fit the main operational buckets yet."
    };

    public static string GetGroupIcon(string groupKey) => groupKey switch
    {
        "records" => "bi bi-briefcase",
        "mni" => "bi bi-person-badge",
        "mli" => "bi bi-geo-alt",
        _ => "bi bi-sliders"
    };

    public static string GetGroupTag(string groupKey) => groupKey switch
    {
        "records" => "Workflow",
        "mni" => "MNI",
        "mli" => "MLI",
        _ => "Other"
    };

    public static int GetGroupOrder(string groupKey) => groupKey switch
    {
        "records" => 0,
        "mni" => 1,
        "mli" => 2,
        _ => 3
    };

    private static string SplitPascalCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length + 8);
        builder.Append(value[0]);

        for (var i = 1; i < value.Length; i++)
        {
            var current = value[i];
            var previous = value[i - 1];

            if (char.IsUpper(current) && (char.IsLower(previous) || char.IsDigit(previous)))
            {
                builder.Append(' ');
            }

            builder.Append(current);
        }

        return builder.ToString();
    }
}
