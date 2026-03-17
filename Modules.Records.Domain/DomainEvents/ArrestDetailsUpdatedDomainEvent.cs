namespace Modules.Records.Domain.DomainEvents;

public sealed record ArrestDetailsUpdatedDomainEvent(
    Guid     ArrestId,
    Guid?    NameId,
    DateTime ArrestedAt,
    Guid?    ArrestTypeId,
    string   ArrestNum,
    Guid?    PrimaryIncidentId) : DomainEvent;
