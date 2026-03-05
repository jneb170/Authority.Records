namespace Modules.Records.Domain.DomainInvariants.IncidentClose;

public sealed class CitationsMustBeIssuedInvariant : IDomainInvariant<IncidentCloseContext>
{
    public const string Code = "incident.close.citations.not_issued";

    public DomainInvariantResult Check(IncidentCloseContext context)
    {
        var unissued = context.Citations.Where(c => !c.IsIssued).ToList();

        if (unissued.Count == 0)
            return DomainInvariantResult.Valid();

        return DomainInvariantResult.Fail(
            Code,
            $"Incident cannot be closed. {unissued.Count} citation(s) have not been issued.");
    }
}
