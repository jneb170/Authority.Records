namespace Modules.Records.Domain.DomainInvariants;

public interface IDomainInvariant<TContext>
{
    DomainInvariantResult Check(TContext context);
}
