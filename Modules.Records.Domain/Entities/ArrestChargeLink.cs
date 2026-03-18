using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Primitives;

namespace Modules.Records.Domain.Entities;

public sealed class ArrestChargeLink : AggregateRoot, IMultiTenant
{
    public Guid JurisdictionId { get; private set; }
    public Guid ArrestId { get; private set; }
    public Guid ChargeId { get; private set; }
    public DateTime LinkedAtUtc { get; private set; }
    public Guid LinkedByUserId { get; private set; }

    private ArrestChargeLink() { } // EF

    public ArrestChargeLink(Guid jurisdictionId, Guid arrestId, Guid chargeId, Guid linkedByUserId)
    {
        Id = Guid.NewGuid();
        JurisdictionId = jurisdictionId;
        ArrestId = arrestId;
        ChargeId = chargeId;
        LinkedAtUtc = DateTime.UtcNow;
        LinkedByUserId = linkedByUserId;
    }
}
