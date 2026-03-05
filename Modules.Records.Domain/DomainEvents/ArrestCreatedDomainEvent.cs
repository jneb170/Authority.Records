namespace Modules.Records.Domain.DomainEvents;

public sealed record ArrestCreatedDomainEvent(
    Guid ArrestId,
    Guid IncidentId,
    Guid JurisdictionId,
    Guid AgencyId,
    string SuspectName,
    DateTime ArrestedAt)
    : DomainEvent;
