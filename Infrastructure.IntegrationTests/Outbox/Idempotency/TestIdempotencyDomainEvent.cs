using Modules.Records.Domain.DomainEvents;

namespace Infrastructure.IntegrationTests.Outbox.Idempotency
{
    public sealed record TestIdempotencyDomainEvent(Guid AggregateId) : IDomainEvent;
}
