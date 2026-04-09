namespace Modules.Records.Domain.DomainEvents;

public sealed record IncidentChargeLinkedDomainEvent(
    Guid LinkId,
    Guid IncidentId,
    Guid ChargeId,
    Guid JurisdictionId,
    Guid LinkedByUserId) : DomainEvent;
