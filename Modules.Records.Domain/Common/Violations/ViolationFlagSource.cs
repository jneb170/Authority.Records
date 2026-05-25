namespace Modules.Records.Domain.Common.Violations;

/// <summary>
/// Where a <see cref="ViolationFlagKey"/> on a citation came from. Today every flag is
/// <see cref="Manual"/> (an officer ticked the box); <see cref="Charge"/> is reserved for a
/// future enhancement where linking a charge auto-derives flags. Stored per flag so the two
/// can coexist and so charge-derived flags can be filtered at read time (e.g. hidden when the
/// originating charge is voided) without a schema change.
/// </summary>
public enum ViolationFlagSource
{
    /// <summary>The flag was set manually by an officer.</summary>
    Manual = 0,

    /// <summary>The flag was derived from a linked charge. See <c>SourceChargeLinkId</c>.</summary>
    Charge = 1
}
