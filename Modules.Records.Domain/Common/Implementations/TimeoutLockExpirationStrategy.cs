using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Primitives;

namespace Modules.Records.Domain.Common;

public sealed class TimeoutLockExpirationStrategy<TAggregate>
    : ILockExpirationStrategy<TAggregate>
    where TAggregate : LockableAggregateRoot<TAggregate>
{
    public bool IsLockActive(
        TAggregate aggregate,
        TimeSpan timeout,
        IClock clock)
    {
        if (!aggregate.IsLocked || aggregate.LockedAtUtc == null)
            return false;

        var expiresAt = aggregate.LockedAtUtc.Value.Add(timeout);
        return expiresAt > clock.UtcNow;
    }
}