using MediatR;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Common;
using Modules.Records.Domain.Common;

namespace Modules.Records.Application.Incidents.Commands.AcquireIncidentLock;

public sealed class AcquireIncidentLockHandler : IRequestHandler<AcquireIncidentLockCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IModificationContext _modificationContext;

    public AcquireIncidentLockHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider, IModificationContext modificationContext)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _modificationContext = modificationContext;
    }

    public async Task<Unit> Handle(AcquireIncidentLockCommand request, CancellationToken cancellationToken)
    {
        var incident = await _dbContext.Incidents
            .FirstOrDefaultAsync(i =>
                i.Id == request.IncidentId &&
                i.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken);

        if (incident == null)
            throw new InvalidOperationException("Incident not found.");

        var lockTimeout = await LockTimeoutResolver.ResolveAsync(
            _dbContext, _tenantProvider.GetAgencyId(), cancellationToken);
        incident.AcquireLock(_modificationContext, lockTimeout);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    Task IRequestHandler<AcquireIncidentLockCommand>.Handle(AcquireIncidentLockCommand request, CancellationToken cancellationToken)
    {
        return Handle(request, cancellationToken);
    }
}
