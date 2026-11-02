using Modules.Records.Application.Common.Queries.GetDashboardCounts;
using Modules.Records.Application.Common.Queries.GetMapMarkers;
using Modules.Records.Application.Common.Queries.GetRecentActivity;

namespace Modules.Records.UI.Services;

public interface IHomeService
{
    Task<IReadOnlyList<RecentActivityDto>> GetRecentActivityAsync(int take = 20);

    /// <summary>Most recent records in the active agency, whoever created or changed them.</summary>
    Task<IReadOnlyList<RecentActivityDto>> GetAgencyRecentActivityAsync(int take = 20);
    Task<IReadOnlyList<MapMarkerDto>> GetMapMarkersAsync(DateTime? since);

    /// <summary>Records created in the active agency over the last <paramref name="days"/> days, plus open incidents.</summary>
    Task<DashboardCountsDto> GetDashboardCountsAsync(int days);
    Task<ActivityMapWindowState> GetDefaultMapWindowAsync();
}
