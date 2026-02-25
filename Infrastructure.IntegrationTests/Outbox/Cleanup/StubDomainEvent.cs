using Modules.Records.Domain.DomainEvents;

public sealed record StubDomainEvent() : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}
