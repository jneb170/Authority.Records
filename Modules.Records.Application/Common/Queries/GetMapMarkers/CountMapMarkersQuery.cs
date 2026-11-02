using MediatR;

namespace Modules.Records.Application.Common.Queries.GetMapMarkers;

/// <summary>
/// Returns the number of map markers (Incidents, Arrests, and Citations with parseable coordinates)
/// for the given jurisdiction whose reference date is on or after <paramref name="Since"/>.
/// Pass <c>null</c> for <paramref name="Since"/> to count all records regardless of date.
/// </summary>
/// <para>
/// <paramref name="Since"/> is a UTC instant (incident <c>OccurredOn</c>, <c>CreatedAtUtc</c>). Arrest
/// <c>ArrestedAt</c> and citation <c>IssueDate</c> are stored as agency wall-clock time, so they are compared
/// with <paramref name="SinceAgencyLocal"/>, the same moment in the agency's time zone (falls back to
/// <paramref name="Since"/> when not supplied).
/// </para>
public sealed record CountMapMarkersQuery(
    Guid      JurisdictionId,
    DateTime? Since,
    DateTime? SinceAgencyLocal = null) : IRequest<int>;
