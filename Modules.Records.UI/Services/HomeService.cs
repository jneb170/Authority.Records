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
}
