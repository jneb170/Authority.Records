using MediatR;

namespace Modules.Records.Application.Common.Queries.GetMapMarkers;

/// <summary>
/// Returns map markers for Incidents, Arrests, and Citations within the given jurisdiction
/// whose reference date is on or after <paramref name="Since"/> and that have a parseable location coordinate.
/// Pass <c>null</c> for <paramref name="Since"/> to return all records regardless of date.
/// </summary>
/// <para>
/// <paramref name="Since"/> is a UTC instant (incident <c>OccurredOn</c>, <c>CreatedAtUtc</c>). Arrest
/// <c>ArrestedAt</c> and citation <c>IssueDate</c> are stored as agency wall-clock time, so they are compared
/// with <paramref name="SinceAgencyLocal"/>, the same moment in the agency's time zone (falls back to
/// <paramref name="Since"/> when not supplied).
/// </para>
public sealed record GetMapMarkersQuery(
    Guid      JurisdictionId,
    DateTime? Since,
    DateTime? SinceAgencyLocal = null) : IRequest<IReadOnlyList<MapMarkerDto>>;
