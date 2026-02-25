namespace Modules.Records.Domain.DomainEvents;

public sealed record IncidentLockReleasedDomainEvent : IDomainEvent
{
    public Guid IncidentId { get; }
    public Guid UserId { get; }
    public DateTime OccurredOnUtc { get; }

    public IncidentLockReleasedDomainEvent(
        Guid incidentId,
        Guid userId,
        DateTime occurredOnUtc)
    {
        IncidentId = incidentId;
        UserId = userId;
        OccurredOnUtc = occurredOnUtc;
    }
}
