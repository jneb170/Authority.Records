using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Primitives;

namespace Modules.Records.Domain.Common.Policies;

public class DefaultClosePolicy<TAggregate>
    : IClosePolicy<TAggregate>
    where TAggregate : AggregateRoot
{
    public virtual void ValidateCanClose(
        TAggregate aggregate,
        bool isForced)
    {
        // No-op by default
    }
}