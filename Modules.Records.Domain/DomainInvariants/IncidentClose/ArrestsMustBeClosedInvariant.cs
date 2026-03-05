using Modules.Records.Domain.Common;

namespace Modules.Records.Domain.DomainInvariants.IncidentClose;

public sealed class ArrestsMustBeClosedInvariant : IDomainInvariant<IncidentCloseContext>
{
    public const string Code = "incident.close.arrests.not_closed";

    public DomainInvariantResult Check(IncidentCloseContext context)
    {
        var open = context.Arrests
            .Where(a => a.Status != RecordStatus.Closed && a.Status != RecordStatus.Archived)
            .ToList();

        if (open.Count == 0)
            return DomainInvariantResult.Valid();

        return DomainInvariantResult.Fail(
            Code,
            $"Incident cannot be closed. {open.Count} arrest(s) are not closed.");
    }
}
