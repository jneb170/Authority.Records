namespace Modules.Records.Domain.Common;

/// <summary>
/// System-provided default item labels seeded when an agency first uses a picklist type.
/// The Value is derived from the label (stripped of spaces and special chars) for stability.
/// </summary>
public static class PicklistDefaults
{
    /// <summary>
    /// Returns default (value, label) pairs for a given picklist type.
    /// Returns an empty list for types with no system defaults (e.g. Court — fully agency-defined).
    /// </summary>
    public static IReadOnlyList<(string Value, string Label)> For(string picklistType) =>
        picklistType switch
        {
            PicklistTypes.ArrestType =>
            [
                ("OnView",        "On View"),
                ("Warrant",       "Warrant"),
                ("Summons",       "Summons"),
                ("Custodial",     "Custodial"),
                ("ProbableCause", "Probable Cause"),
                ("OtherAgency",   "Other Agency"),
            ],
            _ => []
        };
}
