namespace Modules.Records.Domain.DomainEvents;

public sealed record MugshotUnlinkedFromOwnerDomainEvent(
    Guid LinkId,
    Guid MugshotId,
    Guid JurisdictionId,
    string OwnerType,
    Guid OwnerId,
    Guid UnlinkedByUserId) : DomainEvent;
