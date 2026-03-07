namespace Modules.Records.Domain.DomainEvents;

public sealed record AgencyConfigurationSetDomainEvent(
    Guid AgencyConfigurationId,
    Guid JurisdictionId,
    Guid AgencyId,
    string Key,
    string Value)
    : DomainEvent;
