using MediatR;

namespace Modules.Records.Application.Common.Queries.GetMapMarkers;

/// <summary>
/// Returns the number of map markers (Incidents, Arrests, and Citations with parseable coordinates)
/// for the given jurisdiction whose reference date is on or after <paramref name="Since"/>.
/// Pass <c>null</c> for <paramref name="Since"/> to count all records regardless of date.
/// </summary>
public sealed record CountMapMarkersQuery(
    Guid      JurisdictionId,
    DateTime? Since) : IRequest<int>;
