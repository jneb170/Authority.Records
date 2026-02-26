using Modules.Records.Domain.DomainEvents;

namespace Modules.Records.Domain.Common;

    public abstract class LockableAggregateRoot<TAggregate> 
        : StatefulAggregateRoot<TAggregate>
        where TAggregate : LockableAggregateRoot<TAggregate>
{
    public Guid? LockedByUserId { get; protected set; }
    public DateTime? LockedAtUtc { get; protected set; }

    public bool IsLocked =>
        LockedByUserId.HasValue &&
        LockedAtUtc.HasValue;

    // ----------------------------------------------------
    // Public Lock API
    // ----------------------------------------------------

    public virtual void AcquireLock(Guid userId, TimeSpan lockTimeout, bool isSupervisor = false)
    {
        EnsureNotArchived();
        EnsureCanLock(userId);

        if (IsLocked)
        {
            var expiresAt = LockedAtUtc!.Value.Add(lockTimeout);
            var lockActive = expiresAt > DateTime.UtcNow;

            if (lockActive && LockedByUserId != userId && !isSupervisor)
            {
                throw new DomainException(
                    "record.locked",
                    "Record is currently locked by another user.");
            }
        }

        LockedByUserId = userId;
        LockedAtUtc = DateTime.UtcNow;

        AddDomainEvent(new LockAcquiredDomainEvent<TAggregate>(Id, userId));
    }

    public virtual void ReleaseLock(Guid userId, bool isSupervisor = false)
    {
        if (!IsLocked)
            return;

        if (LockedByUserId != userId && !isSupervisor)
        {
            throw new DomainException(
                "record.lock.release.denied",
                "Only the locking user or supervisor can release the lock.");
        }

        LockedByUserId = null;
        LockedAtUtc = null;

        AddDomainEvent(new LockReleasedDomainEvent<TAggregate>(Id, userId));
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

    protected virtual void EnsureCanLock(Guid userId)
    {
        EnsureNotArchived();

        if (IsDeleted)
        {
            throw new DomainException(
                "record.deleted",
                "Deleted records cannot be locked.");
        }

        if (Status == RecordStatus.Closed)
        {
            throw new DomainException(
                "record.closed.lock",
                "Closed records cannot be locked.");
        }
    }

    protected virtual void EnsureCanModify(Guid userId, bool isSupervisor = false)
    {
        EnsureNotArchived();

        // If still Draft and not locked yet, allow modification
        if (Status == RecordStatus.Draft && !IsLocked)
            return;

        if (!IsLocked)
        {
            throw new DomainException(
                "record.lock.required",
                "Record must be locked before modification.");
        }

        if (LockedByUserId != userId && !isSupervisor)
        {
            throw new DomainException(
                "record.lock.ownership",
                "Only the locking user may modify this record.");
        }

        if (Status == RecordStatus.Closed && !isSupervisor)
        {
            throw new DomainException(
                "record.closed.modify",
                "Closed records cannot be modified.");
        }
    }
}

