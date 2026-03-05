namespace Modules.Records.Domain.DomainEvents;

public sealed record CitationCreatedDomainEvent(
    Guid CitationId,
    Guid JurisdictionId,
    Guid AgencyId,
    string Description,
    DateTime IssueDate)
    : DomainEvent;
