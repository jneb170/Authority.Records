using MediatR;
using Modules.Records.Application.Common.Queries.GetDashboardCounts;
using Modules.Records.Application.Common.Queries.GetMapMarkers;
using Modules.Records.Application.Common.Queries.GetRecentActivity;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.UI.Services;

public sealed class HomeService : IHomeService
{
    private readonly ISender _mediator;
    private readonly ITenantProvider _tenantProvider;
    private readonly IAgencyClock _clock;

    public HomeService(ISender mediator, ITenantProvider tenantProvider, IAgencyClock clock)
    {
        _mediator       = mediator;
        _tenantProvider = tenantProvider;
        _clock          = clock;
    }

    public async Task<IReadOnlyList<RecentActivityDto>> GetRecentActivityAsync(int take = 20)
    {
        var query = new GetRecentActivityQuery(
            _tenantProvider.GetUserId(),
            _tenantProvider.GetJurisdictionId(),
            take);

        return await _mediator.Send(query);
    }

    public Task<IReadOnlyList<RecentActivityDto>> GetAgencyRecentActivityAsync(int take = 20) =>
        _mediator.Send(new GetRecentActivityQuery(
            _tenantProvider.GetUserId(),
            _tenantProvider.GetJurisdictionId(),
            take,
            IncludeAllUsers: true));

    public Task<DashboardCountsDto> GetDashboardCountsAsync(int days) =>
        _mediator.Send(new GetDashboardCountsQuery(DateTime.UtcNow.AddDays(-days)));

    public Task<IReadOnlyList<MapMarkerDto>> GetMapMarkersAsync(DateTime? since) =>
        _mediator.Send(new GetMapMarkersQuery(_tenantProvider.GetJurisdictionId(), since, ToAgencyLocal(since)));

    public async Task<ActivityMapWindowState> GetDefaultMapWindowAsync()
    {
        var now           = DateTime.UtcNow;
        // Default to the widest window (0 = all time) as the fallback.
        var selectedHours = ActivityMapWindowSelector.CandidateWindowHours[^1];

        foreach (var hours in ActivityMapWindowSelector.CandidateWindowHours)
        {
            var since = SinceFromWindow(hours, now);
            var count = await _mediator.Send(
                new CountMapMarkersQuery(_tenantProvider.GetJurisdictionId(), since, ToAgencyLocal(since)));

            if (count >= ActivityMapWindowSelector.MinimumMarkers)
            {
                selectedHours = hours;
                break;
            }
        }

        var markers = await GetMapMarkersAsync(SinceFromWindow(selectedHours, now));
        return new ActivityMapWindowState(selectedHours, markers);
    }

    // Arrest and issue times are agency wall-clock values: compare them with the same moment in agency time.
    private DateTime? ToAgencyLocal(DateTime? utc) =>
        utc.HasValue ? _clock.ToAgencyTime(DateTime.SpecifyKind(utc.Value, DateTimeKind.Utc)) : null;

    private static DateTime? SinceFromWindow(int windowHours, DateTime now) =>
        windowHours == 0 ? null : now.AddHours(-windowHours);
}
