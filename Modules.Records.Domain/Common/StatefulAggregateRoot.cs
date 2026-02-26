using Modules.Records.Domain.DomainEvents;

namespace Modules.Records.Domain.Common;

public abstract class StatefulAggregateRoot<TAggregate>
    : AggregateRoot
    where TAggregate : AggregateRoot
{
    public RecordStatus Status { get; protected set; } = RecordStatus.Draft;

    protected void ChangeStatus(
        RecordStatus newStatus,
        Guid userId,
        bool isForced = false)
    {
        if (Status == RecordStatus.Archived)
            throw new DomainException(
                "record.archived",
                "Archived record cannot transition.");

        if (Status == newStatus)
            return;

        ValidateTransition(Status, newStatus, isForced);

        var previous = Status;
        Status = newStatus;

        AddDomainEvent(new LifecycleStatusChangedDomainEvent<TAggregate>(
            AggregateId: Id,
            PreviousStatus: previous,
            NewStatus: newStatus,
            ChangedByUserId: userId,
            IsForced: isForced));
    }

    protected abstract void ValidateTransition(
        RecordStatus current,
        RecordStatus target,
        bool isForced);

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

