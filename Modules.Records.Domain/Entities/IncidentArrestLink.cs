using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Primitives;
using Modules.Records.Domain.DomainEvents;

namespace Modules.Records.Domain.Entities;

public sealed class IncidentArrestLink : AggregateRoot, IMultiTenant
{
    public Guid JurisdictionId { get; private set; }
    public Guid IncidentId { get; private set; }
    public Guid ArrestId { get; private set; }
    public DateTime LinkedAtUtc { get; private set; }
    public Guid LinkedByUserId { get; private set; }

    private IncidentArrestLink() { } // EF

    public IncidentArrestLink(Guid jurisdictionId, Guid incidentId, Guid arrestId, Guid linkedByUserId)
    {
        Id = Guid.NewGuid();
        JurisdictionId = jurisdictionId;
        IncidentId = incidentId;
        ArrestId = arrestId;
        LinkedAtUtc = DateTime.UtcNow;
        LinkedByUserId = linkedByUserId;

        AddDomainEvent(new ArrestLinkedToIncidentDomainEvent(Id, ArrestId, IncidentId, JurisdictionId, LinkedByUserId));
    }

    public void Unlink(Guid unlinkedByUserId)
    {
        AddDomainEvent(new ArrestUnlinkedFromIncidentDomainEvent(Id, ArrestId, IncidentId, JurisdictionId, unlinkedByUserId));
    }
}
