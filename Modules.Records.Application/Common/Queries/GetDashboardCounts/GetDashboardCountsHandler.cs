using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Common.Extensions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;

namespace Modules.Records.Application.Common.Queries.GetDashboardCounts;

public sealed class GetDashboardCountsHandler : IRequestHandler<GetDashboardCountsQuery, DashboardCountsDto>
{
    private static readonly string OpenStatus = RecordStatus.Open.ToString();

    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetDashboardCountsHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<DashboardCountsDto> Handle(GetDashboardCountsQuery request, CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var agencyId = _tenantProvider.GetAgencyId();
        var since = request.SinceUtc;

        // EF Core DbContext is not thread-safe: run the counts sequentially.
        var incidents = _dbContext.IncidentReadModels.AsNoTracking()
            .WhereAgencyScoped(agencyId)
            .Where(r => r.JurisdictionId == jurisdictionId && !r.IsDeleted);

        var newIncidents = await incidents.CountAsync(r => r.CreatedAtUtc >= since, cancellationToken);
        var openIncidents = await incidents.CountAsync(r => r.Status == OpenStatus, cancellationToken);

        var newArrests = await _dbContext.ArrestReadModels.AsNoTracking()
            .WhereAgencyScoped(agencyId)
            .CountAsync(r => r.JurisdictionId == jurisdictionId && r.CreatedAtUtc >= since, cancellationToken);

        var newCitations = await _dbContext.CitationReadModels.AsNoTracking()
            .WhereAgencyScoped(agencyId)
            .CountAsync(r => r.JurisdictionId == jurisdictionId && r.CreatedAtUtc >= since, cancellationToken);

        return new DashboardCountsDto(newIncidents, openIncidents, newArrests, newCitations);
    }
}
