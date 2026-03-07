namespace Modules.Records.Domain.Common;

/// <summary>Well-known picklist type keys used throughout the system.</summary>
public static class PicklistTypes
{
    public const string ArrestType = "ArrestType";
    public const string Court      = "Court";

    /// <summary>All known picklist types in a consistent order for admin UIs.</summary>
    public static readonly IReadOnlyList<string> All = [ArrestType, Court];
}
