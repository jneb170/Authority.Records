using Modules.Records.Domain.DomainEvents;

namespace Infrastructure.IntegrationTests.Outbox.RetryBehavior;

public sealed record FailingDomainEvent(Guid AggregateId) : IDomainEvent;
