using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Locations.Queries.SearchLocations;

public sealed class SearchLocationsHandler
    : IRequestHandler<SearchLocationsQuery, IReadOnlyList<LocationDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public SearchLocationsHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext      = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<IReadOnlyList<LocationDto>> Handle(
        SearchLocationsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.LocationReadModels
            .AsNoTracking()
            .Where(l => l.JurisdictionId == _tenantProvider.GetJurisdictionId());

        if (!string.IsNullOrWhiteSpace(request.AddressContains))
            query = query.Where(l =>
                l.StreetAddress.Contains(request.AddressContains) ||
                (l.CommonPlaceName != null && l.CommonPlaceName.Contains(request.AddressContains)));

        if (!string.IsNullOrWhiteSpace(request.City))
            query = query.Where(l => l.City.Contains(request.City));

        if (request.StateId.HasValue)
            query = query.Where(l => l.StateId == request.StateId);

        if (!string.IsNullOrWhiteSpace(request.Zip))
            query = query.Where(l => l.Zip != null && l.Zip.StartsWith(request.Zip));

        if (!string.IsNullOrWhiteSpace(request.CommonPlaceName))
            query = query.Where(l =>
                l.CommonPlaceName != null && l.CommonPlaceName.Contains(request.CommonPlaceName));

        var results = await query
            .OrderBy(l => l.City)
            .ThenBy(l => l.StreetAddress)
            .Take(100)
            .ToListAsync(cancellationToken);

        return results.Select(r => r.ToDto()).ToList();
    }
}
