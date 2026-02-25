
namespace Modules.Records.Domain.DomainEvents;

public sealed record IncidentRestoredDomainEvent(
    Guid IncidentId,
    Guid UserId)
    : DomainEvent;
