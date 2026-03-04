using Modules.Records.Domain.Common.Primitives;

namespace Modules.Records.Domain.Abstractions;

public interface IAuthorizationPolicy<TAggregate>
    where TAggregate : AggregateRoot
{
    void EnsureCanAcquireLock(
        TAggregate aggregate,
        IModificationContext context);

    void EnsureCanReleaseLock(
        TAggregate aggregate,
        IModificationContext context);

    void EnsureCanModify(
        TAggregate aggregate,
        IModificationContext context);
}