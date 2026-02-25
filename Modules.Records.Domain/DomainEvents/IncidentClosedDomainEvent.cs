
namespace Modules.Records.Domain.DomainEvents;

public sealed record IncidentClosedDomainEvent(
    Guid IncidentId,
    Guid UserId,
    bool forced)
    : DomainEvent;


