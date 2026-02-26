using Modules.Records.Domain.Common;

namespace Modules.Records.Domain.DomainEvents;

public sealed record LockAcquiredDomainEvent<TAggregate>(
    Guid AggregateId,
    Guid UserId
) : DomainEvent
    where TAggregate : AggregateRoot;
