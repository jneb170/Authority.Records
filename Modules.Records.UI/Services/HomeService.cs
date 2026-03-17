using MediatR;
using Modules.Records.Application.Common.Queries.GetMapMarkers;
using Modules.Records.Application.Common.Queries.GetRecentActivity;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.UI.Services;

public sealed class HomeService : IHomeService
{
    private readonly ISender _mediator;
    private readonly ITenantProvider _tenantProvider;

    public HomeService(ISender mediator, ITenantProvider tenantProvider)
    {
        _mediator       = mediator;
        _tenantProvider = tenantProvider;
    }

    public async Task<IReadOnlyList<RecentActivityDto>> GetRecentActivityAsync(int take = 20)
    {
        var query = new GetRecentActivityQuery(
            _tenantProvider.GetUserId(),
            _tenantProvider.GetJurisdictionId(),
            take);

        return await _mediator.Send(query);
    }

    public Task<IReadOnlyList<MapMarkerDto>> GetMapMarkersAsync(DateTime? since) =>
        _mediator.Send(new GetMapMarkersQuery(_tenantProvider.GetJurisdictionId(), since));

    public async Task<ActivityMapWindowState> GetDefaultMapWindowAsync()
    {
        ActivityMapWindowState? fallback = null;

        foreach (var hours in ActivityMapWindowSelector.CandidateWindowHours)
        {
            var markers = await GetMapMarkersAsync(SinceFromWindow(hours));
            var state = new ActivityMapWindowState(hours, markers);

            if (ActivityMapWindowSelector.HasMinimumActivity(markers))
                return state;

            fallback = state;
        }

        return fallback ?? new ActivityMapWindowState(0, Array.Empty<MapMarkerDto>());
    }

    private static DateTime? SinceFromWindow(int windowHours) =>
        windowHours == 0 ? null : DateTime.UtcNow.AddHours(-windowHours);
}
