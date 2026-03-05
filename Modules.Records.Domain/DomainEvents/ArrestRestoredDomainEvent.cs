namespace Modules.Records.Domain.DomainEvents;

public sealed record ArrestRestoredDomainEvent(
    Guid ArrestId,
    Guid UserId)
    : DomainEvent;
