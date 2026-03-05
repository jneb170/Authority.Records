using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Incidents.Commands.ArchiveIncident;

public sealed class ArchiveIncidentHandler : IRequestHandler<ArchiveIncidentCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IModificationContext _modificationContext;
    private readonly ILifecyclePolicy<Incident> _lifecyclePolicy;

    public ArchiveIncidentHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IModificationContext modificationContext,
        ILifecyclePolicy<Incident> lifecyclePolicy)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _modificationContext = modificationContext;
        _lifecyclePolicy = lifecyclePolicy;
    }

    public async Task Handle(ArchiveIncidentCommand request, CancellationToken cancellationToken)
    {
        var incident = await _dbContext.Incidents
            .FirstOrDefaultAsync(i =>
                i.Id == request.IncidentId &&
                i.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Incident not found.");

        incident.Archive(_modificationContext, _lifecyclePolicy);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
