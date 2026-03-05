using Modules.Records.Domain.Common;

namespace Modules.Records.Domain.DomainInvariants.IncidentClose;

public sealed class ArrestsMustBeFinalizedInvariant : IDomainInvariant<IncidentCloseContext>
{
    public const string Code = "incident.close.arrests.not_finalized";

    public DomainInvariantResult Check(IncidentCloseContext context)
    {
        var unfinalized = context.Arrests.Where(a => !a.IsFinalized).ToList();

        if (unfinalized.Count == 0)
            return DomainInvariantResult.Valid();

        return DomainInvariantResult.Fail(
            Code,
            $"Incident cannot be closed. {unfinalized.Count} arrest(s) are not finalized.");
    }
}
