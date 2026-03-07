using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Picklists.Queries.GetPicklistSetting;

public sealed class GetPicklistSettingHandler : IRequestHandler<GetPicklistSettingQuery, PicklistSettingDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetPicklistSettingHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<PicklistSettingDto?> Handle(GetPicklistSettingQuery request, CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var agencyId = _tenantProvider.GetAgencyId();

        var setting = await _dbContext.PicklistSettings
            .FirstOrDefaultAsync(s =>
                s.JurisdictionId == jurisdictionId &&
                s.AgencyId == agencyId &&
                s.PicklistType == request.PicklistType,
                cancellationToken);

        return setting is null
            ? null
            : new PicklistSettingDto(setting.Id, setting.JurisdictionId, setting.AgencyId, setting.PicklistType, setting.IsRequired);
    }
}
