using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Primitives;
using Modules.Records.Domain.DomainEvents;

namespace Modules.Records.Domain.Entities;

public sealed class IncidentCitationLink : AggregateRoot, IMultiTenant
{
    public Guid JurisdictionId { get; private set; }
    public Guid IncidentId { get; private set; }
    public Guid CitationId { get; private set; }
    public DateTime LinkedAtUtc { get; private set; }
    public Guid LinkedByUserId { get; private set; }

    private IncidentCitationLink() { } // EF

    public IncidentCitationLink(Guid jurisdictionId, Guid incidentId, Guid citationId, Guid linkedByUserId)
    {
        Id = Guid.NewGuid();
        JurisdictionId = jurisdictionId;
        IncidentId = incidentId;
        CitationId = citationId;
        LinkedAtUtc = DateTime.UtcNow;
        LinkedByUserId = linkedByUserId;

        AddDomainEvent(new CitationLinkedToIncidentDomainEvent(Id, CitationId, IncidentId, JurisdictionId, LinkedByUserId));
    }

    public void Unlink(Guid unlinkedByUserId)
    {
        AddDomainEvent(new CitationUnlinkedFromIncidentDomainEvent(Id, CitationId, IncidentId, JurisdictionId, unlinkedByUserId));
    }
}
