using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Exceptions;
using Modules.Records.Domain.Common.Specifications.Incident;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Common.Policies;

public sealed class IncidentClosePolicy : IClosePolicy<Incident>
{
    private readonly IJurisdictionRulesService _jurisdictionRules;

    public IncidentClosePolicy(IJurisdictionRulesService jurisdictionRules)
    {
        _jurisdictionRules = jurisdictionRules ?? throw new ArgumentNullException(nameof(jurisdictionRules));
    }

    public void ValidateCanClose(Incident aggregate, bool isForced)
    {
        if (isForced) return;

        if (_jurisdictionRules.MustCloseAllArrests(aggregate.JurisdictionId))
        {
            var spec = new AllArrestsClosedSpecification();
            if (!spec.IsSatisfiedBy(aggregate))
                throw new DomainException(spec.ErrorCode, spec.Reason);
        }
    }
}
