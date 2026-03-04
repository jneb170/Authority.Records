using Modules.Records.Domain.Common.Primitives;

namespace Modules.Records.Domain.Abstractions;

public interface IClosePolicy<TAggregate>
    where TAggregate : AggregateRoot
{
    void ValidateCanClose(
        TAggregate aggregate,
        bool isForced);
}