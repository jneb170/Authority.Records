using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Records.Domain.DomainEvents;

public sealed record IncidentClosedDomainEvent : IDomainEvent
{
    public Guid IncidentId { get; }
    public Guid UserId { get; }
    public DateTime OccurredOnUtc { get; }
    public bool Forced { get; }

    public IncidentClosedDomainEvent(
        Guid incidentId,
        Guid userId,
        DateTime occurredOnUtc,
        bool forced)
    {
        IncidentId = incidentId;
        UserId = userId;
        OccurredOnUtc = occurredOnUtc;
        Forced = forced;
    }
}


