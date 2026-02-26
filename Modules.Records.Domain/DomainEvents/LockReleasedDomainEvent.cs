using Modules.Records.Domain.Common;

namespace Modules.Records.Domain.DomainEvents;

public sealed record LockReleasedDomainEvent<TAggregate>(
    Guid AggregateId,
    Guid UserId
) : DomainEvent
    where TAggregate : AggregateRoot;