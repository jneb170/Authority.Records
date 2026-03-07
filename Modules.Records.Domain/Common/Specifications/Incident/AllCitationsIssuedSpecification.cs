using IncidentEntity = Modules.Records.Domain.Entities.Incident;

namespace Modules.Records.Domain.Common.Specifications.Incident;

// Citation validation is handled by IncidentCloseDomainService via IncidentCanBeClosedInvariant.
// This specification is retained for API compatibility but always returns satisfied.
public sealed class AllCitationsIssuedSpecification : Specification<IncidentEntity>
{
    public override bool IsSatisfiedBy(IncidentEntity entity) => true;

    public override string ErrorCode => "incident.close.citations.not_issued";
    public override string Reason => "All citations must be issued before closing the incident.";
}
