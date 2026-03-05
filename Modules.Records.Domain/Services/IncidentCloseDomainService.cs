using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Exceptions;
using Modules.Records.Domain.DomainInvariants.IncidentClose;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Services;

public sealed class IncidentCloseDomainService
{
    private readonly IArrestRepository _arrestRepository;
    private readonly IncidentCanBeClosedInvariant _invariant;

    public IncidentCloseDomainService(
        IArrestRepository arrestRepository,
        IJurisdictionRulesService jurisdictionRules)
    {
        _arrestRepository = arrestRepository
            ?? throw new ArgumentNullException(nameof(arrestRepository));

        _invariant = new IncidentCanBeClosedInvariant(
            jurisdictionRules ?? throw new ArgumentNullException(nameof(jurisdictionRules)));
    }

    public async Task ValidateCanCloseAsync(
        Incident incident,
        bool isForced,
        CancellationToken cancellationToken)
    {
        if (isForced)
            return;

        var arrests = await _arrestRepository
            .GetByIncidentIdAsync(incident.Id, cancellationToken);

        var citations = (IReadOnlyList<Citation>)incident.Citations.ToList();

        var context = new IncidentCloseContext(incident, arrests, citations);
        var result = _invariant.Check(context);

        if (!result.IsValid)
        {
            var reasons = string.Join(" ", result.Violations.Select(v => v.Reason));
            var code = result.Violations[0].ErrorCode;
            throw new DomainException(code, reasons);
        }
    }
}
