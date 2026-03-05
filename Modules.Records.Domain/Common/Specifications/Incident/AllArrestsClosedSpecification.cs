using IncidentEntity = Modules.Records.Domain.Entities.Incident;
using Modules.Records.Domain.Common;

namespace Modules.Records.Domain.Common.Specifications.Incident;

public sealed class AllArrestsClosedSpecification : Specification<IncidentEntity>
{
    public override bool IsSatisfiedBy(IncidentEntity entity) =>
        entity.Arrests.All(a => a.Status == RecordStatus.Closed);

    public override string ErrorCode => "incident.close.invalid";
    public override string Reason => "All arrests must be closed before closing the incident.";
}
