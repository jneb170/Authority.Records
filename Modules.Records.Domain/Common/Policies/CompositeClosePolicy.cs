using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Exceptions;
using Modules.Records.Domain.Common.Primitives;

namespace Modules.Records.Domain.Common.Policies;

public class CompositeClosePolicy<TAggregate>
    : IClosePolicy<TAggregate>
    where TAggregate : AggregateRoot
{
    private readonly IReadOnlyList<IClosePolicy<TAggregate>> _policies;

    public CompositeClosePolicy(
        IEnumerable<IClosePolicy<TAggregate>> policies)
    {
        if (policies == null)
            throw new ArgumentNullException(nameof(policies));

        _policies = policies.ToList().AsReadOnly();
    }

    public void ValidateCanClose(
        TAggregate aggregate,
        bool isForced)
    {
        var exceptions = new List<DomainException>();

        foreach (var policy in _policies)
        {
            try
            {
                policy.ValidateCanClose(aggregate, isForced);
            }
            catch (AggregateDomainException aggEx)
            {
                // Flatten nested aggregates
                exceptions.AddRange(aggEx.Errors);
            }
            catch (DomainException ex)
            {
                exceptions.Add(ex);
            }
        }

        if (exceptions.Any())
        {
            throw new AggregateDomainException(exceptions);
        }
    }
}