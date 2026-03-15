namespace Modules.Records.Domain.DomainEvents;

public sealed record MugshotRestoredDomainEvent(
    Guid MugshotId,
    Guid RestoredByUserId) : DomainEvent;
