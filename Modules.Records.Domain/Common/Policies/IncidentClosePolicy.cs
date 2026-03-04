using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Exceptions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Common.Policies;

public sealed class IncidentClosePolicy
    : IClosePolicy<Incident>
{
    private readonly IJurisdictionRulesService _jurisdictionRules;

    public IncidentClosePolicy(IJurisdictionRulesService jurisdictionRules)
    {
        _jurisdictionRules = jurisdictionRules ?? throw new ArgumentNullException(nameof(jurisdictionRules));
    }

    public void ValidateCanClose(Incident aggregate, bool isForced)
    {
        if (!isForced)
        {
            // Example: incident cannot close if any child Arrest is still open
            if (_jurisdictionRules.MustCloseAllArrests(aggregate.JurisdictionId))
            {
                if (aggregate.Arrests.Any(a => a.Status != RecordStatus.Closed))
                    throw new DomainException(
                        "incident.close.invalid",
                        "All arrests must be closed before closing the incident.");
            }

            // Other child collection rules for Citations, Vehicles, etc. can go here
        }
    }
}