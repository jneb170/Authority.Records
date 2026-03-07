namespace Modules.Records.Domain.DomainEvents;

public sealed record CitationLinkedToIncidentDomainEvent(
    Guid LinkId,
    Guid CitationId,
    Guid IncidentId,
    Guid JurisdictionId,
    Guid LinkedByUserId)
    : DomainEvent;
