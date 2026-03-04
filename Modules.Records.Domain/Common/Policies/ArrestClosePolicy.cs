using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Exceptions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Common.Policies;

public sealed class ArrestClosePolicy
    : IClosePolicy<Arrest>
{
    private readonly IJurisdictionRulesService _jurisdictionRules;

    public ArrestClosePolicy(IJurisdictionRulesService jurisdictionRules)
    {
        _jurisdictionRules = jurisdictionRules ?? throw new ArgumentNullException(nameof(jurisdictionRules));
    }

    public void ValidateCanClose(
        Arrest aggregate,
        bool isForced)
    {
        if (!isForced)
        {
            if (string.IsNullOrWhiteSpace(aggregate.SuspectName))
                throw new DomainException("arrest.close.invalid", "Suspect name required before closing.");

            if (aggregate.ArrestedAt > DateTime.UtcNow)
                throw new DomainException("arrest.date.invalid", "Arrest date cannot be in the future.");

            if (_jurisdictionRules.MustCloseAllArrests(aggregate.JurisdictionId) && !aggregate.IsFinalized)
                throw new DomainException(
                    "arrest.close.invalid",
                    "All arrests must be finalized before closing.");
        }


        // Future:
        // - Must have at least one officer
        // - Must have at least one charge
    }
}