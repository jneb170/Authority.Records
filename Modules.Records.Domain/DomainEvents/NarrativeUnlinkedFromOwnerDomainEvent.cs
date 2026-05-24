namespace Modules.Records.Domain.DomainEvents;

public sealed record NarrativeUnlinkedFromOwnerDomainEvent(
    Guid LinkId,
    Guid NarrativeId,
    Guid JurisdictionId,
    string OwnerType,
    Guid OwnerId,
    Guid UserId) : DomainEvent;
