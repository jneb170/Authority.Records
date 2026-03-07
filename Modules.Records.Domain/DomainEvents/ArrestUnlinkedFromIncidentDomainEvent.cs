namespace Modules.Records.Domain.DomainEvents;

public sealed record ArrestUnlinkedFromIncidentDomainEvent(
    Guid LinkId,
    Guid ArrestId,
    Guid IncidentId,
    Guid JurisdictionId,
    Guid UnlinkedByUserId)
    : DomainEvent;
