using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Arrests.Queries.GetArrestsByJurisdiction;

public sealed class GetArrestsByJurisdictionHandler
    : IRequestHandler<GetArrestsByJurisdictionQuery, IReadOnlyList<ArrestDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetArrestsByJurisdictionHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<IReadOnlyList<ArrestDto>> Handle(
        GetArrestsByJurisdictionQuery request,
        CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var results = await _dbContext.ArrestReadModels
            .AsNoTracking()
            .Where(a => a.JurisdictionId == jurisdictionId)
            .OrderByDescending(a => a.ArrestedAt)
            .ToListAsync(cancellationToken);

        return await ArrestDtoMapper.ToDtosAsync(results, _dbContext, cancellationToken);
    }
}
