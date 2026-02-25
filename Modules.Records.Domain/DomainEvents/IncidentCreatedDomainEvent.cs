
namespace Modules.Records.Domain.DomainEvents;

public sealed record IncidentCreatedDomainEvent(
    Guid IncidentId,
    Guid UserId)
    : DomainEvent;



