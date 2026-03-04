using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Exceptions;
using Modules.Records.Domain.DomainEvents;

namespace Modules.Records.Domain.Common.Primitives;

public abstract class LockableAggregateRoot<TAggregate>
    : StatefulAggregateRoot<TAggregate>
    where TAggregate : LockableAggregateRoot<TAggregate>
{
    protected abstract IAuthorizationPolicy<TAggregate> AuthorizationPolicy { get; }
    protected abstract ILockExpirationStrategy<TAggregate> LockExpirationStrategy { get; }
    protected abstract IClock Clock { get; }

    public Guid? LockedByUserId { get; protected set; }
    public DateTime? LockedAtUtc { get; protected set; }
    public bool IsLocked =>
        LockedByUserId.HasValue &&
        LockedAtUtc.HasValue;

    // ----------------------------------------------------
    // Public Lock API
    // ----------------------------------------------------

    public virtual void AcquireLock(IModificationContext context, TimeSpan lockTimeout)
    {
        EnsureNotArchived();

        AuthorizationPolicy.EnsureCanAcquireLock(
            (TAggregate)this,
            context);

        if (IsLocked)
        {
            var lockActive = LockExpirationStrategy.IsLockActive(
                (TAggregate)this,
                lockTimeout,
                Clock);

            if (lockActive &&
                LockedByUserId != context.UserId &&
                !context.CanOverrideLocks)
            {
                throw new DomainException(
                    "record.locked",
                    "Record is currently locked by another user.");
            }
        }

        LockedByUserId = context.UserId;
        LockedAtUtc = DateTime.UtcNow;

        AddDomainEvent(new LockAcquiredDomainEvent<TAggregate>(Id, context.UserId));
    }

    public virtual void ReleaseLock(IModificationContext context)
    {
        AuthorizationPolicy.EnsureCanReleaseLock(
            (TAggregate)this,
            context);

        if (!IsLocked)
            return;

        LockedByUserId = null;
        LockedAtUtc = null;

        AddDomainEvent(new LockReleasedDomainEvent<TAggregate>(Id, context.UserId));
    }

    protected virtual void EnsureCanModify(IModificationContext context)
    {
        AuthorizationPolicy.EnsureCanModify(
            (TAggregate)this,
            context);
    }

    protected virtual void EnsureUserOwnsLock(Guid userId)
    {
        if (!IsLocked || LockedByUserId != userId)
        {
            throw new DomainException(
                "record.lock.required",
                "User must own the lock to modify this record.");
        }
    }

    protected virtual void EnsureCanLock(IModificationContext context)
    {
        EnsureNotArchived();

        if (IsDeleted)
        {
            throw new DomainException(
                "record.deleted",
                "Deleted records cannot be locked.");
        }

        if (Status == RecordStatus.Closed
            && !context.CanOverrideLocks)
        {
            throw new DomainException(
                "record.closed.lock",
                "Closed records cannot be locked.");
        }
    }


}

