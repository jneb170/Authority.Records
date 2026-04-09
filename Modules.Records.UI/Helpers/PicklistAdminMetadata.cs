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
        PicklistTypes.ViolationSourceType => "Violation Source Type",
        PicklistTypes.ViolationGroup => "Violation Group",
        PicklistTypes.SpeedBand => "Speed Band",
        PicklistTypes.MovementViolation => "Movement Violation",
        PicklistTypes.ParkingViolation => "Parking Violation",
        PicklistTypes.EnvironmentFactor => "Environment Factor",
        PicklistTypes.CollisionConfiguration => "Collision Configuration",
        PicklistTypes.IncidentSeverity => "Incident Severity",
        PicklistTypes.AreaType => "Area Type",
        PicklistTypes.HighwayType => "Highway Type",
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
        PicklistTypes.ViolationSourceType => "Texas citation authority sources such as state statute and local ordinance.",
        PicklistTypes.ViolationGroup => "Primary Texas citation offense groupings used for print mapping.",
        PicklistTypes.SpeedBand => "Texas citation speeding ranges used when a speed violation is recorded.",
        PicklistTypes.MovementViolation => "Texas citation movement-related checkbox values.",
        PicklistTypes.ParkingViolation => "Texas citation parking-related checkbox values.",
        PicklistTypes.EnvironmentFactor => "Texas citation surface, visibility, traffic, and avoidance factors.",
        PicklistTypes.CollisionConfiguration => "Texas citation collision diagram and impact configuration values.",
        PicklistTypes.IncidentSeverity => "Texas citation incident severity values such as PD, PI, and Fatal.",
        PicklistTypes.AreaType => "Texas citation area-context values such as business, school, and rural.",
        PicklistTypes.HighwayType => "Texas citation roadway type values used for roadway description.",
        _ => "Additional configurable dropdown values used across records."
    };

    public static string GetGroupKey(string type) => type switch
    {
        PicklistTypes.Race or PicklistTypes.Sex or PicklistTypes.Suffix or PicklistTypes.HairColor or PicklistTypes.EyeColor or PicklistTypes.State
            => "mni",
        PicklistTypes.Direction or PicklistTypes.StreetType or PicklistTypes.Country
            => "mli",
        PicklistTypes.ArrestType or PicklistTypes.Court or PicklistTypes.ViolationSourceType or PicklistTypes.ViolationGroup
            or PicklistTypes.SpeedBand or PicklistTypes.MovementViolation or PicklistTypes.ParkingViolation
            or PicklistTypes.EnvironmentFactor or PicklistTypes.CollisionConfiguration or PicklistTypes.IncidentSeverity
            or PicklistTypes.AreaType or PicklistTypes.HighwayType
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
