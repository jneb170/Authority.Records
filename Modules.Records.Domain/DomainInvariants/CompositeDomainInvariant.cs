namespace Modules.Records.Domain.DomainInvariants;

public sealed class CompositeDomainInvariant<TContext> : IDomainInvariant<TContext>
{
    private readonly IEnumerable<IDomainInvariant<TContext>> _invariants;

    public CompositeDomainInvariant(IEnumerable<IDomainInvariant<TContext>> invariants)
    {
        _invariants = invariants;
    }

    public DomainInvariantResult Check(TContext context)
    {
        var violations = _invariants
            .SelectMany(i => i.Check(context).Violations)
            .ToList();

        return new DomainInvariantResult(violations);
    }
}
