namespace Modules.Records.Domain.DomainEvents;

public sealed record ChargeCreatedDomainEvent(
    Guid ChargeId,
    Guid JurisdictionId,
    Guid AgencyId,
    string OffenseName,
    string UcrCode,
    string ChargeLevel,
    bool IsCitationEligible) : DomainEvent;
