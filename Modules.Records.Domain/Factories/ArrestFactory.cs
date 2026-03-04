using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Factories;

public sealed class ArrestFactory
{
    public Arrest Create(Guid jurisdictionId, Guid agencyId, Guid incidentId, string suspectName, DateTime arrestedAt)
        => new Arrest(jurisdictionId, agencyId, incidentId, suspectName, arrestedAt);
}
