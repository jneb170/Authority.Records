using Modules.Records.Domain.Common.Exceptions;

namespace Modules.Records.Domain.ValueObjects;

/// <summary>
/// Encapsulates all freely-editable data fields on an Incident.
/// To add a new field: add one property here (+ to <see cref="Validate"/> if constrained).
/// All commands, events, DTOs, and handlers thread this object through unchanged.
/// </summary>
public sealed record IncidentDetails
{
    public required string IncidentNum { get; init; }
    public required string LocalNum { get; init; }
    public required string Description { get; init; }
    public          string CFSNum      { get; init; } = string.Empty;

    // C# records give structural equality for free — no GetEqualityComponents() needed.

    /// <summary>Validates business rules. Called by the entity before applying.</summary>
    public IncidentDetails Validate()
    {
        if (string.IsNullOrWhiteSpace(IncidentNum))
            throw new DomainException("incident.incidentnum.empty", "IncidentNum is required.");

        if (string.IsNullOrWhiteSpace(Description))
            throw new DomainException("incident.description.empty", "Description is required.");

        if (CFSNum.Length > 30)
            throw new DomainException("incident.cfsnum.length", "CFSNum must not exceed 30 characters.");

        return this;
    }
}
