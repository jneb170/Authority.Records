using Modules.Records.Domain.DomainEvents;

namespace Infrastructure.IntegrationTests.Outbox.Idempotency
{
    public sealed record TestIdempotencyDomainEvent(Guid AggregateId) : IDomainEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
        public long AggregateVersion { get; init; } = 0;
    }
}
