using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Primitives;
using Modules.Records.Domain.DomainEvents;

namespace Modules.Records.Domain.Entities;

public sealed class AgencyConfiguration : AggregateRoot, IMultiTenant
{
    public Guid JurisdictionId { get; private set; }
    public Guid AgencyId { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;

    private AgencyConfiguration() { } // EF

    public AgencyConfiguration(Guid jurisdictionId, Guid agencyId, string key, string value)
    {
        Id = Guid.NewGuid();
        JurisdictionId = jurisdictionId;
        AgencyId = agencyId;
        Key = key;
        Value = value;

        AddDomainEvent(new AgencyConfigurationSetDomainEvent(Id, JurisdictionId, AgencyId, Key, Value));
    }

    public void Update(string newValue)
    {
        Value = newValue;
        AddDomainEvent(new AgencyConfigurationSetDomainEvent(Id, JurisdictionId, AgencyId, Key, Value));
    }
}
