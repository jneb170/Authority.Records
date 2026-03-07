using IncidentEntity = Modules.Records.Domain.Entities.Incident;

namespace Modules.Records.Domain.Common.Specifications.Incident;

// Arrest/citation validation is handled by IncidentCloseDomainService via IncidentCanBeClosedInvariant.
// This specification is retained for API compatibility but always returns satisfied.
public sealed class AllArrestsFinalizedSpecification : Specification<IncidentEntity>
{
    public override bool IsSatisfiedBy(IncidentEntity entity) => true;

    public override string ErrorCode => "incident.close.arrests.not_finalized";
    public override string Reason => "All arrests must be finalized before closing the incident.";
}
