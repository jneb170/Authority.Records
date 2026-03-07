using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Picklists.Commands.SetPicklistSetting;

public sealed class SetPicklistSettingHandler : IRequestHandler<SetPicklistSettingCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public SetPicklistSettingHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(SetPicklistSettingCommand request, CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var agencyId = _tenantProvider.GetAgencyId();

        var setting = await _dbContext.PicklistSettings
            .FirstOrDefaultAsync(s =>
                s.JurisdictionId == jurisdictionId &&
                s.AgencyId == agencyId &&
                s.PicklistType == request.PicklistType,
                cancellationToken);

        if (setting is null)
        {
            setting = new PicklistSetting(jurisdictionId, agencyId, request.PicklistType, request.IsRequired);
            _dbContext.PicklistSettings.Add(setting);
        }
        else
        {
            setting.SetRequired(request.IsRequired);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
