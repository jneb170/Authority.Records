using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Records.Domain.DomainEvents;

public sealed record IncidentOpenedDomainEvent : IDomainEvent
{
    public Guid IncidentId { get; }
    public Guid UserId { get; }
    public DateTime OccurredOnUtc { get; }

    public IncidentOpenedDomainEvent(
        Guid incidentId,
        Guid userId,
        DateTime occurredOnUtc)
    {
        IncidentId = incidentId;
        UserId = userId;
        OccurredOnUtc = occurredOnUtc;
    }
}


