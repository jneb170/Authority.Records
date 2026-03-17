namespace Modules.Records.Domain.DomainEvents;

public sealed record ArrestCreatedDomainEvent(
    Guid ArrestId,
    Guid JurisdictionId,
    Guid? NameId,
    DateTime ArrestedAt,
    string ArrestNum,
    Guid? PrimaryIncidentId)
    : DomainEvent;
