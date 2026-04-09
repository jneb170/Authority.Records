namespace Modules.Records.Domain.DomainEvents;

public sealed record MugshotSoftDeletedDomainEvent(
    Guid MugshotId,
    Guid DeletedByUserId) : DomainEvent;
