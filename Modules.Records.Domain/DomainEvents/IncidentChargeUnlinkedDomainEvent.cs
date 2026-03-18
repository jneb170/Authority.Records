namespace Modules.Records.Domain.DomainEvents;

public sealed record IncidentChargeUnlinkedDomainEvent(
    Guid LinkId,
    Guid IncidentId,
    Guid ChargeId,
    Guid JurisdictionId,
    Guid UnlinkedByUserId) : DomainEvent;
