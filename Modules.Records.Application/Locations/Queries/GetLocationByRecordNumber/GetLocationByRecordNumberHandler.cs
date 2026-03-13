using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Locations.Queries.GetLocationByRecordNumber;

public sealed class GetLocationByRecordNumberHandler : IRequestHandler<GetLocationByRecordNumberQuery, LocationDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetLocationByRecordNumberHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext      = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<LocationDto?> Handle(
        GetLocationByRecordNumberQuery request,
        CancellationToken cancellationToken)
    {
        var rm = await _dbContext.LocationReadModels
            .AsNoTracking()
            .FirstOrDefaultAsync(l =>
                l.RecordNumber == request.RecordNumber &&
                l.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken);

        return rm?.ToDto();
    }
}
