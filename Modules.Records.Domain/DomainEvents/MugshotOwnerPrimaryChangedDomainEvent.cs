namespace Modules.Records.Domain.DomainEvents;

public sealed record MugshotOwnerPrimaryChangedDomainEvent(
    Guid LinkId,
    Guid MugshotId,
    Guid JurisdictionId,
    string OwnerType,
    Guid OwnerId,
    bool IsPrimary,
    int DisplayOrder) : DomainEvent;
