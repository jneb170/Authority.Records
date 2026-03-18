namespace Modules.Records.Domain.DomainEvents;

public sealed record ChargeUpdatedDomainEvent(
    Guid ChargeId,
    string OffenseName,
    string UcrCategory,
    string NibrsGroup,
    string CrimeAgainst,
    string UcrCode,
    string ChargeLevel,
    string? StateClass,
    bool IsCitationEligible) : DomainEvent;
