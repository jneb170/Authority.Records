namespace Modules.Records.Domain.Common.Violations;

/// <summary>
/// Logical grouping of a <see cref="ViolationFlagKey"/>, mirroring the boxed panels on the
/// printed citation form so entry and print stay 1:1. Jurisdiction-neutral.
/// </summary>
public enum ViolationFlagSection
{
    /// <summary>The main offense checkbox grid (turning, lane, signal, etc.).</summary>
    Offense = 0,

    /// <summary>"Contributors to last violation" (weather/light conditions).</summary>
    Contributor = 1,

    /// <summary>"Caused person to dodge".</summary>
    Dodge = 2,

    /// <summary>"Type of collision".</summary>
    Collision = 3
}
