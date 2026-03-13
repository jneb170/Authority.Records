using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Locations.Queries.GetLocationsByJurisdiction;

public sealed class GetLocationsByJurisdictionHandler
    : IRequestHandler<GetLocationsByJurisdictionQuery, IReadOnlyList<LocationDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetLocationsByJurisdictionHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext      = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<IReadOnlyList<LocationDto>> Handle(
        GetLocationsByJurisdictionQuery request,
        CancellationToken cancellationToken)
    {
        var results = await _dbContext.LocationReadModels
            .AsNoTracking()
            .Where(l => l.JurisdictionId == _tenantProvider.GetJurisdictionId())
            .OrderBy(l => l.City)
            .ThenBy(l => l.StreetAddress)
            .ToListAsync(cancellationToken);

        return results.Select(r => r.ToDto()).ToList();
    }
}
