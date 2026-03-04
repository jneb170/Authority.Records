using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Primitives;

namespace Modules.Records.Domain.Abstractions;

public interface ILifecyclePolicy<TAggregate>
where TAggregate : AggregateRoot
{
    void ValidateTransition(
        TAggregate aggregate,
        RecordStatus current,
        RecordStatus target,
        IModificationContext context,
        bool isForced);
}
