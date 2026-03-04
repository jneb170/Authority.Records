using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Exceptions;
using Modules.Records.Domain.DomainEvents;

namespace Modules.Records.Domain.Common.Primitives;

public abstract class StatefulAggregateRoot<TAggregate>
    : AggregateRoot
    where TAggregate : StatefulAggregateRoot<TAggregate>
{

    public RecordStatus Status { get; protected set; } = RecordStatus.Draft;

    protected void ChangeStatus(
        RecordStatus newStatus,
        IModificationContext context,
        ILifecyclePolicy<TAggregate> lifecyclePolicy,
        bool isForced = false)
    {
        if (Status == RecordStatus.Archived)
            throw new DomainException(
                "record.archived",
                "Archived record cannot transition.");

        if (Status == newStatus)
            return;

        lifecyclePolicy.ValidateTransition(
            (TAggregate)this,
            Status,
            newStatus,
            context,
            isForced);

        var previous = Status;
        Status = newStatus;

        AddDomainEvent(new LifecycleStatusChangedDomainEvent<TAggregate>(
            AggregateId: Id,
            PreviousStatus: previous,
            NewStatus: newStatus,
            ChangedByUserId: context.UserId,
            IsForced: isForced));
    }

    protected void EnsureNotArchived()
    {
        if (Status == RecordStatus.Archived)
        {
            throw new DomainException(
                "record.archived",
                "Archived records cannot be modified.");
        }
    }

    protected void EnsureNotClosed(bool allowIfForced = false)
    {
        if (Status == RecordStatus.Closed && !allowIfForced)
        {
            throw new DomainException(
                "record.closed",
                "Closed records cannot be modified.");
        }
    }
}

