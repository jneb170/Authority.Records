using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Factories;

public sealed class IncidentFactory
{
    public Incident Create(Guid jurisdictionId, Guid agencyId, string description)
        => new Incident(jurisdictionId, agencyId, description);
}
