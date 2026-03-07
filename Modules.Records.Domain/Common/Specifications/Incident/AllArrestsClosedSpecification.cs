using IncidentEntity = Modules.Records.Domain.Entities.Incident;
using Modules.Records.Domain.Common;

namespace Modules.Records.Domain.Common.Specifications.Incident;

// Arrest/citation validation is handled by IncidentCloseDomainService via IncidentCanBeClosedInvariant.
// This specification is retained for API compatibility but always returns satisfied.
public sealed class AllArrestsClosedSpecification : Specification<IncidentEntity>
{
    public override bool IsSatisfiedBy(IncidentEntity entity) => true;

    public override string ErrorCode => "incident.close.invalid";
    public override string Reason => "All arrests must be closed before closing the incident.";
}
