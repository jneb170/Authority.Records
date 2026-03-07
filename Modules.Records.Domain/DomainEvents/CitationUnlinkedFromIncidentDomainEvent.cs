namespace Modules.Records.Domain.DomainEvents;

public sealed record CitationUnlinkedFromIncidentDomainEvent(
    Guid LinkId,
    Guid CitationId,
    Guid IncidentId,
    Guid JurisdictionId,
    Guid UnlinkedByUserId)
    : DomainEvent;
