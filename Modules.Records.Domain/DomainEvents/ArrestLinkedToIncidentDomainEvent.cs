namespace Modules.Records.Domain.DomainEvents;

public sealed record ArrestLinkedToIncidentDomainEvent(
    Guid LinkId,
    Guid ArrestId,
    Guid IncidentId,
    Guid JurisdictionId,
    Guid LinkedByUserId)
    : DomainEvent;
