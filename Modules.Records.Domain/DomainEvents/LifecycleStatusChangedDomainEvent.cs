using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Primitives;

namespace Modules.Records.Domain.DomainEvents;

/// <summary>
/// Raised when an aggregate moves between lifecycle states (Draft, Open, Closed, Archived).
/// </summary>
/// <remarks>
/// The constructor assigns the inherited <see cref="DomainEvent.AggregateId"/> so the value survives the
/// outbox's JSON round-trip (System.Text.Json binds it through this constructor; the base setter is internal).
/// </remarks>
public sealed record LifecycleStatusChangedDomainEvent<TAggregate> : DomainEvent
    where TAggregate : AggregateRoot
{
    public LifecycleStatusChangedDomainEvent(
        Guid aggregateId,
        RecordStatus previousStatus,
        RecordStatus newStatus,
        Guid changedByUserId,
        bool isForced)
    {
        AggregateId = aggregateId;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        ChangedByUserId = changedByUserId;
        IsForced = isForced;
    }

    public RecordStatus PreviousStatus { get; init; }
    public RecordStatus NewStatus { get; init; }
    public Guid ChangedByUserId { get; init; }
    public bool IsForced { get; init; }
}
