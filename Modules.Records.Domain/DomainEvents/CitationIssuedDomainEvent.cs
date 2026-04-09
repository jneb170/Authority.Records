namespace Modules.Records.Domain.DomainEvents;

public sealed record CitationIssuedDomainEvent(
    Guid CitationId,
    Guid IssuedByUserId) : DomainEvent;
