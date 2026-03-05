using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Exceptions;
using Modules.Records.Domain.Common.Specifications;
using Modules.Records.Domain.Common.Specifications.Citation;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Common.Policies;

public sealed class CitationClosePolicy : IClosePolicy<Citation>
{
    private readonly IJurisdictionRulesService _jurisdictionRules;

    public CitationClosePolicy(IJurisdictionRulesService jurisdictionRules)
    {
        _jurisdictionRules = jurisdictionRules ?? throw new ArgumentNullException(nameof(jurisdictionRules));
    }

    public void ValidateCanClose(Citation aggregate, bool isForced)
    {
        if (isForced) return;

        var specs = new List<ISpecification<Citation>>
        {
            new IssueDateNotFutureSpecification()
        };

        var errors = specs
            .Where(s => !s.IsSatisfiedBy(aggregate))
            .Select(s => new DomainException(s.ErrorCode, s.Reason))
            .ToList();

        if (_jurisdictionRules.MustCloseAllCitations(aggregate.JurisdictionId))
            errors.Add(new DomainException(
                "citation.close.invalid",
                "All citations must have charges before closing."));

        if (errors.Any())
            throw new AggregateDomainException(errors);
    }
}
