namespace Modules.Records.Domain.DomainEvents;

public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
    public Guid AggregateId { get; internal set; }
    public long AggregateVersion { get; internal set; }
}