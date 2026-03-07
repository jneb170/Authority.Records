namespace Modules.Records.Domain.DomainEvents;

public sealed record ArrestCreatedDomainEvent(
    Guid ArrestId,
    Guid JurisdictionId,
    string SuspectName,
    DateTime ArrestedAt,
    string ArrestNum)
    : DomainEvent;
