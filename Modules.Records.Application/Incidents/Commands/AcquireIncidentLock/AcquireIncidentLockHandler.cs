using MediatR;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using Modules.Records.Application.Abstractions;

namespace Modules.Records.Application.Incidents.Commands.AcquireIncidentLock;

public sealed class AcquireIncidentLockHandler : IRequestHandler<AcquireIncidentLockCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public AcquireIncidentLockHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
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

        // Acquire lock (10 minute timeout for example)
        incident.AcquireLock(_tenantProvider.GetAgencyId(), TimeSpan.FromMinutes(10));

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    Task IRequestHandler<AcquireIncidentLockCommand>.Handle(AcquireIncidentLockCommand request, CancellationToken cancellationToken)
    {
        return Handle(request, cancellationToken);
    }
}
