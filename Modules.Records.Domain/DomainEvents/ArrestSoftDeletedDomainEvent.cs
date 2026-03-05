namespace Modules.Records.Domain.DomainEvents;

public sealed record ArrestSoftDeletedDomainEvent(
    Guid ArrestId,
    Guid UserId)
    : DomainEvent;
