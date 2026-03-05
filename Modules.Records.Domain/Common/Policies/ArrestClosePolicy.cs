using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Exceptions;
using Modules.Records.Domain.Common.Specifications;
using Modules.Records.Domain.Common.Specifications.Arrest;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Common.Policies;

public sealed class ArrestClosePolicy : IClosePolicy<Arrest>
{
    private readonly IJurisdictionRulesService _jurisdictionRules;

    public ArrestClosePolicy(IJurisdictionRulesService jurisdictionRules)
    {
        _jurisdictionRules = jurisdictionRules ?? throw new ArgumentNullException(nameof(jurisdictionRules));
    }

    public void ValidateCanClose(Arrest aggregate, bool isForced)
    {
        if (isForced) return;

        var specs = new List<ISpecification<Arrest>>
        {
            new SuspectNameProvidedSpecification(),
            new ArrestDateNotFutureSpecification()
        };

        if (_jurisdictionRules.MustCloseAllArrests(aggregate.JurisdictionId))
            specs.Add(new ArrestFinalizedSpecification());

        var errors = specs
            .Where(s => !s.IsSatisfiedBy(aggregate))
            .Select(s => new DomainException(s.ErrorCode, s.Reason))
            .ToList();

        if (errors.Any())
            throw new AggregateDomainException(errors);
    }
}
