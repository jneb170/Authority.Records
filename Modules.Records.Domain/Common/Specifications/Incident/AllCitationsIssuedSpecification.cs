using IncidentEntity = Modules.Records.Domain.Entities.Incident;

namespace Modules.Records.Domain.Common.Specifications.Incident;

public sealed class AllCitationsIssuedSpecification : Specification<IncidentEntity>
{
    public override bool IsSatisfiedBy(IncidentEntity entity) =>
        entity.Citations.All(c => c.IsIssued);

    public override string ErrorCode => "incident.close.citations.not_issued";
    public override string Reason => "All citations must be issued before closing the incident.";
}
