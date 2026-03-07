namespace Modules.Records.Domain.DomainEvents;

public sealed record CitationCreatedDomainEvent(
    Guid CitationId,
    Guid JurisdictionId,
    string Description,
    DateTime IssueDate,
    string CitationNum)
    : DomainEvent;
