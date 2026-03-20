using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Common.Extensions;
using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Incidents.Queries.GetIncidentsByJurisdiction;

public sealed class GetIncidentsByJurisdictionHandler
    : IRequestHandler<GetIncidentsByJurisdictionQuery, IReadOnlyList<IncidentDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetIncidentsByJurisdictionHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<IReadOnlyList<IncidentDto>> Handle(
        GetIncidentsByJurisdictionQuery request,
        CancellationToken cancellationToken)
    {
        var items = await _dbContext.IncidentReadModels
            .AsNoTracking()
            .WhereAgencyScoped(_tenantProvider.GetAgencyId())
            .Where(i => i.JurisdictionId == request.JurisdictionId && !i.IsDeleted)
            .OrderByDescending(i => i.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

        return items.Select(rm => rm.ToDto()).ToList();
    }
}
