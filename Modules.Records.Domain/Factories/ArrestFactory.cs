using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Factories;

public sealed class ArrestFactory
{
    public Arrest Create(Guid jurisdictionId, Guid agencyId, string suspectName, DateTime arrestedAt, string arrestNum)
        => new Arrest(jurisdictionId, agencyId, suspectName, arrestedAt, arrestNum);
}
