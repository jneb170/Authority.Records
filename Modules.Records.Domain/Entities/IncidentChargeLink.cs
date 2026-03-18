using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Primitives;

namespace Modules.Records.Domain.Entities;

public sealed class IncidentChargeLink : AggregateRoot, IMultiTenant
{
    public Guid JurisdictionId { get; private set; }
    public Guid IncidentId { get; private set; }
    public Guid ChargeId { get; private set; }
    public DateTime LinkedAtUtc { get; private set; }
    public Guid LinkedByUserId { get; private set; }

    private IncidentChargeLink() { } // EF

    public IncidentChargeLink(Guid jurisdictionId, Guid incidentId, Guid chargeId, Guid linkedByUserId)
    {
        Id = Guid.NewGuid();
        JurisdictionId = jurisdictionId;
        IncidentId = incidentId;
        ChargeId = chargeId;
        LinkedAtUtc = DateTime.UtcNow;
        LinkedByUserId = linkedByUserId;
    }
}
