namespace Modules.Records.Domain.Common;

/// <summary>Well-known picklist type keys used throughout the system.</summary>
public static class PicklistTypes
{
    public const string ArrestType = "ArrestType";
    public const string Court      = "Court";

    // Master Name Index (MNI) picklist types
    public const string Race      = "Race";
    public const string Sex       = "Sex";
    public const string State     = "State";     // Reusable: DL state, Address state, etc.
    public const string HairColor = "HairColor";
    public const string EyeColor  = "EyeColor";

    /// <summary>All known picklist types in a consistent order for admin UIs.</summary>
    public static readonly IReadOnlyList<string> All =
        [ArrestType, Court, Race, Sex, State, HairColor, EyeColor];
}
