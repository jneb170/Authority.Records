using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Exceptions;
using Modules.Records.Domain.Common.Primitives;

namespace Modules.Records.Domain.Common.Policies;

public class DefaultAuthorizationPolicy<TAggregate>
    : IAuthorizationPolicy<TAggregate>
    where TAggregate : LockableAggregateRoot<TAggregate>
{
    public virtual void EnsureCanAcquireLock(
        TAggregate aggregate,
        IModificationContext context)
    {
        if (aggregate.IsDeleted)
        {
            throw new DomainException(
                "record.deleted",
                "Deleted records cannot be locked.");
        }

        if (aggregate.Status == RecordStatus.Closed &&
            !context.CanOverrideLocks)
        {
            throw new DomainException(
                "record.closed.lock",
                "Closed records cannot be locked.");
        }
    }

    public virtual void EnsureCanReleaseLock(
        TAggregate aggregate,
        IModificationContext context)
    {
        if (!aggregate.IsLocked)
            return;

        if (aggregate.LockedByUserId != context.UserId &&
            !context.CanOverrideLocks)
        {
            throw new DomainException(
                "record.lock.release.denied",
                "Only the locking user or authorized user can release the lock.");
        }
    }

    public virtual void EnsureCanModify(
        TAggregate aggregate,
        IModificationContext context)
    {
        if (aggregate.Status == RecordStatus.Archived)
        {
            throw new DomainException(
                "record.archived",
                "Archived records cannot be modified.");
        }

        if (aggregate.Status == RecordStatus.Draft && !aggregate.IsLocked)
            return;

        if (!aggregate.IsLocked)
        {
            throw new DomainException(
                "record.lock.required",
                "Record must be locked before modification.");
        }

        if (aggregate.LockedByUserId != context.UserId &&
            !context.CanOverrideLocks)
        {
            throw new DomainException(
                "record.lock.ownership",
                "Only the locking user may modify this record.");
        }

        if (aggregate.Status == RecordStatus.Closed &&
            !context.CanModifyClosedRecords)
        {
            throw new DomainException(
                "record.closed.modify",
                "Closed records cannot be modified.");
        }
    }
}