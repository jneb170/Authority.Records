namespace Modules.Records.Domain.DomainEvents;

public sealed record ArrestChargeUnlinkedDomainEvent(
    Guid LinkId,
    Guid ArrestId,
    Guid ChargeId,
    Guid JurisdictionId,
    Guid UnlinkedByUserId) : DomainEvent;
