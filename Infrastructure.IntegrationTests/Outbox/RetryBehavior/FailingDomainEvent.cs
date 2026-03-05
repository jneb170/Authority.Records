using Modules.Records.Domain.DomainEvents;

namespace Infrastructure.IntegrationTests.Outbox.RetryBehavior;

public sealed record FailingDomainEvent(Guid AggregateId) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
    public long AggregateVersion { get; init; } = 0;
}
