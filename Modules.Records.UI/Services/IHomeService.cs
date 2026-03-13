using Modules.Records.Application.Common.Queries.GetMapMarkers;
using Modules.Records.Application.Common.Queries.GetRecentActivity;

namespace Modules.Records.UI.Services;

public interface IHomeService
{
    Task<IReadOnlyList<RecentActivityDto>> GetRecentActivityAsync(int take = 20);
    Task<IReadOnlyList<MapMarkerDto>> GetMapMarkersAsync(DateTime? since);
}
