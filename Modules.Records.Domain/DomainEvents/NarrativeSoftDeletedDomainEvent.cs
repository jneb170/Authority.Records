namespace Modules.Records.Domain.DomainEvents;

public sealed record NarrativeSoftDeletedDomainEvent(
    Guid NarrativeId,
    Guid UserId) : DomainEvent;
