namespace Modules.Records.Domain.DomainEvents;

public sealed record ArrestChargeLinkedDomainEvent(
    Guid LinkId,
    Guid ArrestId,
    Guid ChargeId,
    Guid JurisdictionId,
    Guid LinkedByUserId) : DomainEvent;
