using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Configurations.Queries.GetAgencyConfiguration;

public sealed class GetAgencyConfigurationHandler
    : IRequestHandler<GetAgencyConfigurationQuery, AgencyConfigurationDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetAgencyConfigurationHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<AgencyConfigurationDto?> Handle(
        GetAgencyConfigurationQuery request, CancellationToken cancellationToken)
    {
        var agencyId = _tenantProvider.GetAgencyId();

        return await _dbContext.AgencyConfigurations
            .AsNoTracking()
            .Where(c => c.AgencyId == agencyId && c.Key == request.Key)
            .Select(c => new AgencyConfigurationDto(c.Id, c.Key, c.Value, c.ModifiedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
