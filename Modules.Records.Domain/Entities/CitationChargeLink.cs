using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Primitives;

namespace Modules.Records.Domain.Entities;

public sealed class CitationChargeLink : AggregateRoot, IMultiTenant
{
    public Guid JurisdictionId { get; private set; }
    public Guid CitationId { get; private set; }
    public Guid ChargeId { get; private set; }
    public DateTime LinkedAtUtc { get; private set; }
    public Guid LinkedByUserId { get; private set; }

    private CitationChargeLink() { } // EF

    public CitationChargeLink(Guid jurisdictionId, Guid citationId, Guid chargeId, Guid linkedByUserId)
    {
        Id = Guid.NewGuid();
        JurisdictionId = jurisdictionId;
        CitationId = citationId;
        ChargeId = chargeId;
        LinkedAtUtc = DateTime.UtcNow;
        LinkedByUserId = linkedByUserId;
    }
}
