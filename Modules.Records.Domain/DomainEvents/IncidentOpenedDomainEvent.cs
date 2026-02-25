
namespace Modules.Records.Domain.DomainEvents;

public sealed record IncidentOpenedDomainEvent(
    Guid IncidentId,
    Guid UserId)
    : DomainEvent;