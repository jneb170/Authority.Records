using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Configurations.Commands.DeleteAgencyConfiguration;

public sealed class DeleteAgencyConfigurationHandler : IRequestHandler<DeleteAgencyConfigurationCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public DeleteAgencyConfigurationHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(DeleteAgencyConfigurationCommand request, CancellationToken cancellationToken)
    {
        var agencyId = _tenantProvider.GetAgencyId();
        var userId = _tenantProvider.GetUserId();

        var config = await _dbContext.AgencyConfigurations
            .FirstOrDefaultAsync(c => c.AgencyId == agencyId && c.Key == request.Key, cancellationToken);

        if (config is null)
            return;

        config.SoftDelete(userId);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
