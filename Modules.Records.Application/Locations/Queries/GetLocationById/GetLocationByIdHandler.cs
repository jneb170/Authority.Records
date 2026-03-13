using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Locations.Queries.GetLocationById;

public sealed class GetLocationByIdHandler : IRequestHandler<GetLocationByIdQuery, LocationDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetLocationByIdHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext      = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<LocationDto?> Handle(GetLocationByIdQuery request, CancellationToken cancellationToken)
    {
        var rm = await _dbContext.LocationReadModels
            .AsNoTracking()
            .FirstOrDefaultAsync(l =>
                l.Id == request.Id &&
                l.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken);

        return rm?.ToDto();
    }
}
