using Modules.Records.Domain.ValueObjects;

namespace Modules.Records.Domain.Factories;

/// <summary>
/// Captures all data required when constructing a new Incident.
/// Add new fields to <see cref="IncidentDetails"/> — this class never changes.
/// </summary>
public sealed class CreateIncidentRequest
{
    public required Guid            JurisdictionId { get; init; }
    public required Guid            AgencyId       { get; init; }
    public required IncidentDetails Details        { get; init; }
}
