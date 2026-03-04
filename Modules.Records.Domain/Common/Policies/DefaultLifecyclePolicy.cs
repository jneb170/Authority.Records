using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Exceptions;
using Modules.Records.Domain.Common.Primitives;

namespace Modules.Records.Domain.Common.Policies;

public class DefaultLifecyclePolicy<TAggregate>
    : ILifecyclePolicy<TAggregate>
    where TAggregate : AggregateRoot
{
    private readonly IClosePolicy<TAggregate> _closePolicy;

    public DefaultLifecyclePolicy(
        IClosePolicy<TAggregate> closePolicy)
    {
        _closePolicy = closePolicy
            ?? throw new ArgumentNullException(nameof(closePolicy));
    }

    private static readonly IReadOnlyDictionary<RecordStatus, RecordStatus[]> AllowedTransitions
        = new Dictionary<RecordStatus, RecordStatus[]>
        {
            { RecordStatus.Draft, new[] { RecordStatus.Open } },
            { RecordStatus.Open, new[] { RecordStatus.Closed } },
            { RecordStatus.Closed, new[] { RecordStatus.Archived } },
            { RecordStatus.Archived, Array.Empty<RecordStatus>() }
        };

    public void ValidateTransition(
        TAggregate aggregate,
        RecordStatus current,
        RecordStatus target,
        IModificationContext context,
        bool isForced)
    {
        if (current == target)
            return;

        if (!AllowedTransitions.TryGetValue(current, out var allowed) ||
            !allowed.Contains(target))
        {
            throw new DomainException(
                "record.invalid.transition",
                $"Invalid transition from {current} to {target}");
        }

        // Delegate close validation
        if (current == RecordStatus.Open &&
            target == RecordStatus.Closed)
        {
            _closePolicy.ValidateCanClose(aggregate, isForced);
        }

        ValidateAdditionalRules(aggregate, current, target, isForced);
    }

    protected virtual void ValidateAdditionalRules(
        TAggregate aggregate,
        RecordStatus current,
        RecordStatus target,
        bool isForced)
    {
        // Optional for derived policies
    }
}