using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Common.Extensions;
using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Arrests.Queries.GetArrestsByIncident;

public sealed class GetArrestsByIncidentHandler
    : IRequestHandler<GetArrestsByIncidentQuery, IReadOnlyList<ArrestDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetArrestsByIncidentHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<IReadOnlyList<ArrestDto>> Handle(
        GetArrestsByIncidentQuery request,
        CancellationToken cancellationToken)
    {
        var arrestIds = await _dbContext.IncidentArrestLinkReadModels
            .AsNoTracking()
            .Where(l => l.IncidentId == request.IncidentId)
            .Select(l => l.ArrestId)
            .ToListAsync(cancellationToken);

        var results = await _dbContext.ArrestReadModels
            .AsNoTracking()
            .WhereAgencyScoped(_tenantProvider.GetAgencyId())
            .Where(a => arrestIds.Contains(a.Id))
            .ToListAsync(cancellationToken);

        return await ArrestDtoMapper.ToDtosAsync(results, _dbContext, cancellationToken);
    }
}
