using MediatR;

namespace Modules.Records.Application.Common.Queries.GetMapMarkers;

/// <summary>
/// Returns map markers for Incidents, Arrests, and Citations within the given jurisdiction
/// whose reference date is on or after <paramref name="Since"/> and that have a parseable location coordinate.
/// Pass <c>null</c> for <paramref name="Since"/> to return all records regardless of date.
/// </summary>
public sealed record GetMapMarkersQuery(
    Guid      JurisdictionId,
    DateTime? Since) : IRequest<IReadOnlyList<MapMarkerDto>>;
