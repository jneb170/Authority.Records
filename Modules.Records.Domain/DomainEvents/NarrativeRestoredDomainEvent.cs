namespace Modules.Records.Domain.DomainEvents;

public sealed record NarrativeRestoredDomainEvent(
    Guid NarrativeId,
    Guid UserId) : DomainEvent;
