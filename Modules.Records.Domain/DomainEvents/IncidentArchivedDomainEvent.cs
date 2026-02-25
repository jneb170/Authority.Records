
namespace Modules.Records.Domain.DomainEvents;

public sealed record IncidentArchivedDomainEvent(
    Guid IncidentId,
    Guid UserId) 
    : DomainEvent;



