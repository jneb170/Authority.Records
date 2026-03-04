using Modules.Records.Domain.Common.Primitives;

namespace Modules.Records.Domain.Abstractions;

public interface ILockExpirationStrategy<TAggregate>
    where TAggregate : LockableAggregateRoot<TAggregate>
{
    bool IsLockActive(
        TAggregate aggregate,
        TimeSpan timeout,
        IClock clock);
}