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
        var now           = DateTime.UtcNow;
        // Default to the widest window (0 = all time) as the fallback.
        var selectedHours = ActivityMapWindowSelector.CandidateWindowHours[^1];

        foreach (var hours in ActivityMapWindowSelector.CandidateWindowHours)
        {
            var count = await _mediator.Send(
                new CountMapMarkersQuery(_tenantProvider.GetJurisdictionId(), SinceFromWindow(hours, now)));

            if (count >= ActivityMapWindowSelector.MinimumMarkers)
            {
                selectedHours = hours;
                break;
            }
        }

        var markers = await GetMapMarkersAsync(SinceFromWindow(selectedHours, now));
        return new ActivityMapWindowState(selectedHours, markers);
    }

    private static DateTime? SinceFromWindow(int windowHours, DateTime now) =>
        windowHours == 0 ? null : now.AddHours(-windowHours);
}
