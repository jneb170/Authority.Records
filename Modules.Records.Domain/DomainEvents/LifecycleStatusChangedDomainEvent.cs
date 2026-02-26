using Modules.Records.Domain.Common;

namespace Modules.Records.Domain.DomainEvents;

public sealed record LifecycleStatusChangedDomainEvent<TAggregate>(
        Guid AggregateId,
        RecordStatus PreviousStatus,
        RecordStatus NewStatus,
        Guid ChangedByUserId,
        bool IsForced
    ) : DomainEvent
        where TAggregate : AggregateRoot;
