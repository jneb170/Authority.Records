namespace Modules.Records.Domain.Common;

    public abstract class LockableAggregateRoot : StatefulAggregateRoot
{
    public Guid? LockedByUserId { get; protected set; }
    public DateTime? LockedAtUtc { get; protected set; }

    public bool IsLocked =>
        LockedByUserId.HasValue &&
        LockedAtUtc.HasValue;

    // ----------------------------------------------------
    // Public Lock API
    // ----------------------------------------------------

    public void AcquireLock(Guid userId, TimeSpan lockTimeout, bool isSupervisor = false)
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

        AddDomainEvent(CreateLockAcquiredEvent(userId, LockedAtUtc.Value));
    }

    public void ReleaseLock(Guid userId, bool isSupervisor = false)
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

        AddDomainEvent(CreateLockReleasedEvent(userId));
    }

    protected void EnsureUserOwnsLock(Guid userId)
    {
        if (!IsLocked || LockedByUserId != userId)
        {
            throw new DomainException(
                "record.lock.required",
                "User must own the lock to modify this record.");
        }
    }

    // ----------------------------------------------------
    // Extension Hooks
    // ----------------------------------------------------

    protected virtual void EnsureCanLock(Guid userId) { }

    protected abstract object CreateLockAcquiredEvent(Guid userId, DateTime lockedAtUtc);
    protected abstract object CreateLockReleasedEvent(Guid userId);
}

