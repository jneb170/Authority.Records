using Modules.Records.Domain.Common.Primitives;

namespace Modules.Records.Domain.DomainEvents;

/// <summary>
/// Raised when the pessimistic lock on an aggregate is released.
/// </summary>
/// <remarks>
/// The constructor assigns the inherited <see cref="DomainEvent.AggregateId"/> so the value survives the
/// outbox's JSON round-trip (System.Text.Json binds it through this constructor; the base setter is internal).
/// </remarks>
public sealed record LockReleasedDomainEvent<TAggregate> : DomainEvent
    where TAggregate : AggregateRoot
{
    public LockReleasedDomainEvent(Guid aggregateId, Guid userId)
    {
        AggregateId = aggregateId;
        UserId = userId;
    }

    public Guid UserId { get; init; }
}
