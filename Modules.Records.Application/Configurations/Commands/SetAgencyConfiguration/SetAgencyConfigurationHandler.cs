using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Configurations.Commands.SetAgencyConfiguration;

public sealed class SetAgencyConfigurationHandler : IRequestHandler<SetAgencyConfigurationCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public SetAgencyConfigurationHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<Guid> Handle(SetAgencyConfigurationCommand request, CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var agencyId = _tenantProvider.GetAgencyId();

        var existing = await _dbContext.AgencyConfigurations
            .FirstOrDefaultAsync(c => c.AgencyId == agencyId && c.Key == request.Key, cancellationToken);

        if (existing is not null)
        {
            existing.Update(request.Value);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return existing.Id;
        }

        var config = new AgencyConfiguration(jurisdictionId, agencyId, request.Key, request.Value);
        _dbContext.AgencyConfigurations.Add(config);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return config.Id;
    }
}
