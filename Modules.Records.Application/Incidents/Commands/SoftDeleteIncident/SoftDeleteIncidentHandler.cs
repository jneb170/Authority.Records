using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Common;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Incidents.Commands.SoftDeleteIncident;

public sealed class SoftDeleteIncidentHandler : IRequestHandler<SoftDeleteIncidentCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IModificationContext _modificationContext;

    public SoftDeleteIncidentHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IModificationContext modificationContext)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _modificationContext = modificationContext;
    }

    public async Task Handle(SoftDeleteIncidentCommand request, CancellationToken cancellationToken)
    {
        var incident = await _dbContext.Incidents
            .FirstOrDefaultAsync(i =>
                i.Id == request.IncidentId &&
                i.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Incident not found.");

        // Drafts only, and never while another user holds an active lock (issue #122).
        var lockTimeout = await LockTimeoutResolver.ResolveAsync(
            _dbContext, _tenantProvider.GetAgencyId(), cancellationToken);
        incident.Delete(_modificationContext, lockTimeout);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
