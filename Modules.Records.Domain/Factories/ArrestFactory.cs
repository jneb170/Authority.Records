using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Factories;

public sealed class ArrestFactory
{
    public Arrest Create(Guid jurisdictionId, Guid agencyId, Guid? nameId, DateTime arrestedAt, string arrestNum, Guid? primaryIncidentId = null)
        => new Arrest(jurisdictionId, agencyId, nameId, arrestedAt, arrestNum, primaryIncidentId);
}
