
namespace Modules.Records.Domain.DomainEvents;

public sealed record IncidentSoftDeletedDomainEvent(
    Guid IncidentId,
    Guid UserId)
    : DomainEvent;