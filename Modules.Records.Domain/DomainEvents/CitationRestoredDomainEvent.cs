namespace Modules.Records.Domain.DomainEvents;

public sealed record CitationRestoredDomainEvent(
    Guid CitationId,
    Guid UserId)
    : DomainEvent;
