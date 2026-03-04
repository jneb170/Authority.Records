using Modules.Records.Domain.Common.Primitives;

namespace Modules.Records.Domain.DomainEvents;

public sealed record LockAcquiredDomainEvent<TAggregate>(
    Guid AggregateId,
    Guid UserId
) : DomainEvent
    where TAggregate : AggregateRoot;
