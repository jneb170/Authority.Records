using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Violations;

namespace Modules.Records.Domain.Entities;

/// <summary>
/// A single structured violation checkbox set on a citation. Stored as one row per set flag (keyed
/// off the citation) so flags are individually queryable and the form layout can be rearranged
/// without a migration. <see cref="Source"/> + <see cref="SourceChargeLinkId"/> record provenance so
/// charge-derived flags (a future enhancement) can coexist with manual ones and be filtered at read
/// time. Like the other citation supplemental entities this is a plain <see cref="IMultiTenant"/>
/// entity (not an aggregate root): no domain events, cascade-deleted with its citation, and scoped by
/// <see cref="CitationId"/> rather than by a global tenant query filter.
/// </summary>
public sealed class CitationViolationFlag : IMultiTenant
{
    public Guid Id { get; private set; }
    public Guid JurisdictionId { get; private set; }
    public Guid AgencyId { get; private set; }
    public Guid CitationId { get; private set; }
    public ViolationFlagKey Key { get; private set; }
    public ViolationFlagSource Source { get; private set; }

    /// <summary>The charge link this flag was derived from, when <see cref="Source"/> is Charge; otherwise null.</summary>
    public Guid? SourceChargeLinkId { get; private set; }

    private CitationViolationFlag()
    {
    }

    public CitationViolationFlag(
        Guid jurisdictionId,
        Guid agencyId,
        Guid citationId,
        ViolationFlagKey key,
        ViolationFlagSource source = ViolationFlagSource.Manual,
        Guid? sourceChargeLinkId = null)
    {
        Id = Guid.NewGuid();
        JurisdictionId = jurisdictionId;
        AgencyId = agencyId;
        CitationId = citationId;
        Key = key;
        Source = source;
        SourceChargeLinkId = sourceChargeLinkId;
    }
}
