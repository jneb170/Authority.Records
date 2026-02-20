using System;

namespace Modules.Records.Domain.DomainEvents;

public sealed class IncidentLockAcquiredDomainEvent : IDomainEvent
{
    public Guid IncidentId { get; }
    public Guid UserId { get; }
    public DateTime OccurredOnUtc { get; }

    public IncidentLockAcquiredDomainEvent(
        Guid incidentId,
        Guid userId,
        DateTime occurredOnUtc)
    {
        IncidentId = incidentId;
        UserId = userId;
        OccurredOnUtc = occurredOnUtc;
    }
}
