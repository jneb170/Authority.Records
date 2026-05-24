using Modules.Records.Domain.Common.Exceptions;

namespace Modules.Records.Domain.ValueObjects;

/// <summary>
/// Encapsulates all freely-editable data fields on an Incident.
/// To add a new field: add one property here (+ to <see cref="Validate"/> if constrained).
/// All commands, events, DTOs, and handlers thread this object through unchanged.
/// </summary>
public sealed record IncidentDetails
{
    /// <summary>
    /// Hard upper bound on the free-text Description. The DB column is unbounded
    /// (nvarchar(max)/TEXT), so this is the only thing stopping a single edit from
    /// stuffing the narrative with megabytes. Generous enough for any real narrative;
    /// the create form applies a tighter limit of its own.
    /// </summary>
    public const int MaxDescriptionLength = 50_000;

    public required string IncidentNum { get; init; }
    public required string LocalNum { get; init; }
    public          string Description { get; init; } = string.Empty;
    public          string CFSNum      { get; init; } = string.Empty;

    // C# records give structural equality for free — no GetEqualityComponents() needed.

    /// <summary>Validates business rules. Called by the entity before applying.</summary>
    public IncidentDetails Validate()
    {
        if (string.IsNullOrWhiteSpace(IncidentNum))
            throw new DomainException("incident.incidentnum.empty", "IncidentNum is required.");

        if(CFSNum.Length > 30)
            throw new DomainException("incident.cfsnum.length", "CFSNum must not exceed 30 characters.");

        if (Description.Length > MaxDescriptionLength)
            throw new DomainException(
                "incident.description.length",
                $"Description must not exceed {MaxDescriptionLength:N0} characters.");

        return this;
    }
}
