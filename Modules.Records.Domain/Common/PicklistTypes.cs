namespace Modules.Records.Domain.Common;

/// <summary>Well-known picklist type keys used throughout the system.</summary>
public static class PicklistTypes
{
    public const string ArrestType = "ArrestType";
    public const string Court      = "Court";

    // Master Name Index (MNI) picklist types
    public const string Race      = "Race";
    public const string Sex       = "Sex";
    public const string Suffix    = "Suffix";    // Name suffix: JR, SR, II, III, MD, etc.
    public const string State     = "State";     // Reusable: DL state, Address state, etc.
    public const string HairColor = "HairColor";
    public const string EyeColor  = "EyeColor";

    // Master Location Index (MLI) picklist types
    public const string Direction  = "Direction";   // Street pre/post direction: N, NE, E, SE, S, SW, W, NW
    public const string StreetType = "StreetType";  // St, Ave, Blvd, Dr, Ct, Ln, Pl, Rd, Way, Hwy, Pkwy
    public const string Country    = "Country";     // US, CA, MX, etc.

    /// <summary>All known picklist types in a consistent order for admin UIs.</summary>
    public static readonly IReadOnlyList<string> All =
        [ArrestType, Court, Race, Sex, Suffix, State, HairColor, EyeColor, Direction, StreetType, Country];
}
