using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;
using Modules.Records.Domain.Services;

namespace Modules.Records.Application.Incidents.Commands.CloseIncident;

public sealed class CloseIncidentHandler : IRequestHandler<CloseIncidentCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IModificationContext _modificationContext;
    private readonly ILifecyclePolicy<Incident> _lifecyclePolicy;
    private readonly IncidentCloseDomainService _closeDomainService;

    public CloseIncidentHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IModificationContext modificationContext,
        ILifecyclePolicy<Incident> lifecyclePolicy,
        IncidentCloseDomainService closeDomainService)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _modificationContext = modificationContext;
        _lifecyclePolicy = lifecyclePolicy;
        _closeDomainService = closeDomainService;
    }

    public async Task Handle(CloseIncidentCommand request, CancellationToken cancellationToken)
    {
        var incident = await _dbContext.Incidents
            .Include(i => i.Citations)
            .FirstOrDefaultAsync(i =>
                i.Id == request.IncidentId &&
                i.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Incident not found.");

        await _closeDomainService.ValidateCanCloseAsync(incident, request.Force, cancellationToken);

        incident.Close(_modificationContext, _lifecyclePolicy, request.Force);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
