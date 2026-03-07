namespace Modules.Records.Domain.DomainEvents;

public sealed record CitationDetailsUpdatedDomainEvent(
    Guid     CitationId,
    string   Description,
    DateTime IssueDate) : DomainEvent;
