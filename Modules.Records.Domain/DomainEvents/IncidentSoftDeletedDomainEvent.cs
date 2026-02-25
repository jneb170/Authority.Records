namespace Modules.Records.Domain.DomainEvents;

public sealed record IncidentSoftDeletedDomainEvent : IDomainEvent
{
    public Guid IncidentId { get; }
    public Guid UserId { get; }
    public DateTime OccurredOnUtc { get; }

    public IncidentSoftDeletedDomainEvent(
        Guid incidentId,
        Guid userId,
        DateTime occurredOnUtc)
    {
        IncidentId = incidentId;
        UserId = userId;
        OccurredOnUtc = occurredOnUtc;
    }
}
