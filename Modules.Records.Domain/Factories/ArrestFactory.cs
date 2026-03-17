using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Factories;

public sealed class ArrestFactory
{
    public Arrest Create(Guid jurisdictionId, Guid agencyId, Guid? nameId, DateTime arrestedAt, string arrestNum, Guid? primaryIncidentId = null)
        => new Arrest(jurisdictionId, agencyId, nameId, arrestedAt, arrestNum, primaryIncidentId);

    public Arrest Create(Guid jurisdictionId, Guid agencyId, string suspectName, DateTime arrestedAt, string arrestNum)
        => new Arrest(jurisdictionId, agencyId, null, arrestedAt, arrestNum, null);
}
