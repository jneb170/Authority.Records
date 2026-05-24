namespace Modules.Records.Domain.DomainEvents;

public sealed record NarrativeLinkedToOwnerDomainEvent(
    Guid LinkId,
    Guid NarrativeId,
    Guid JurisdictionId,
    string OwnerType,
    Guid OwnerId,
    int DisplayOrder) : DomainEvent;
