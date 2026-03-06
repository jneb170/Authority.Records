namespace Modules.Records.Domain.DomainEvents;

public sealed record CitationCreatedDomainEvent(
    Guid CitationId,
    Guid JurisdictionId,
    Guid AgencyId,
    Guid IncidentId,
    string Description,
    DateTime IssueDate)
    : DomainEvent;
