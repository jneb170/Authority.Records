namespace Modules.Records.Domain.DomainEvents;

public sealed record ArrestDetailsUpdatedDomainEvent(
    Guid     ArrestId,
    string   SuspectName,
    DateTime ArrestedAt,
    Guid?    ArrestTypeId,
    string   ArrestNum) : DomainEvent;
