
namespace Modules.Records.Domain.DomainEvents;

public sealed record IncidentLockAcquiredDomainEvent(
    Guid IncidentId,
    Guid UserId)
    : DomainEvent;