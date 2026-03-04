using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Exceptions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Common.Policies;

public sealed class CitationClosePolicy
    : IClosePolicy<Citation>
{
    private readonly IJurisdictionRulesService _jurisdictionRules;

    public CitationClosePolicy(IJurisdictionRulesService jurisdictionRules)
    {
        _jurisdictionRules = jurisdictionRules ?? throw new ArgumentNullException(nameof(jurisdictionRules));
    }

    public void ValidateCanClose(
        Citation aggregate,
        bool isForced)
    {
        if (!isForced)
        {
            if (aggregate.IssueDate > DateTime.UtcNow)
                throw new DomainException("citation.date.invalid", "Citation Issue Date cannot be in the future.");

            if (_jurisdictionRules.MustCloseAllCitations(aggregate.JurisdictionId))
                throw new DomainException(
                    "citation.close.invalid",
                    "All citations must have charges before closing.");
        }

        // Future:
        // - Must have at least one officer
        // - Must have at least one charge
    }
}