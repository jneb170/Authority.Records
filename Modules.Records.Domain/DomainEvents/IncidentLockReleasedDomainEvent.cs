
namespace Modules.Records.Domain.DomainEvents;

public sealed record IncidentLockReleasedDomainEvent(
    Guid IncidentId,
    Guid UserId)
    : DomainEvent;