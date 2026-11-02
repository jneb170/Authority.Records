using MediatR;

namespace Modules.Records.Application.Common.Queries.GetDashboardCounts;

/// <summary>
/// Real record counts for the dashboard tiles in the active agency (issue #100): records created since
/// <see cref="SinceUtc"/>, plus how many incidents are currently open. The tiles used to count rows by type
/// within the 20-item activity feed, so they always added up to 20 or less.
/// </summary>
public sealed record GetDashboardCountsQuery(DateTime SinceUtc) : IRequest<DashboardCountsDto>;

public sealed record DashboardCountsDto(
    int NewIncidents,
    int OpenIncidents,
    int NewArrests,
    int NewCitations);
