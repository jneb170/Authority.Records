using IncidentEntity = Modules.Records.Domain.Entities.Incident;

namespace Modules.Records.Domain.Common.Specifications.Incident;

public sealed class AllArrestsFinalizedSpecification : Specification<IncidentEntity>
{
    public override bool IsSatisfiedBy(IncidentEntity entity) =>
        entity.Arrests.All(a => a.IsFinalized);

    public override string ErrorCode => "incident.close.arrests.not_finalized";
    public override string Reason => "All arrests must be finalized before closing the incident.";
}
