using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Domain.DomainInvariants.IncidentClose;

public sealed class IncidentCanBeClosedInvariant : IDomainInvariant<IncidentCloseContext>
{
    private readonly IJurisdictionRulesService _jurisdictionRules;

    public IncidentCanBeClosedInvariant(IJurisdictionRulesService jurisdictionRules)
    {
        _jurisdictionRules = jurisdictionRules
            ?? throw new ArgumentNullException(nameof(jurisdictionRules));
    }

    public DomainInvariantResult Check(IncidentCloseContext context)
    {
        var invariants = new List<IDomainInvariant<IncidentCloseContext>>
        {
            new ArrestsMustBeFinalizedInvariant(),
            new CitationsMustBeIssuedInvariant()
        };

        if (_jurisdictionRules.MustCloseAllArrests(context.Incident.JurisdictionId))
            invariants.Add(new ArrestsMustBeClosedInvariant());

        return new CompositeDomainInvariant<IncidentCloseContext>(invariants).Check(context);
    }
}
