namespace Modules.Records.Domain.DomainEvents;

public sealed record CitationSoftDeletedDomainEvent(
    Guid CitationId,
    Guid UserId)
    : DomainEvent;
