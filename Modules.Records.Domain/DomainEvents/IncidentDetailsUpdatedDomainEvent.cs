using Modules.Records.Domain.ValueObjects;

namespace Modules.Records.Domain.DomainEvents;

/// <summary>
/// Raised when editable details on an incident are updated.
/// Carries the full <see cref="IncidentDetails"/> — never add individual field params here.
/// </summary>
public sealed record IncidentDetailsUpdatedDomainEvent(
    Guid            IncidentId,
    IncidentDetails Details,
    DateTime?       OccurredOn = null,
    Guid?           LocationId = null,
    Guid?           ModifiedBy = null) : DomainEvent;
