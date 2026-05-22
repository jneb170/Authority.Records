using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Common;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Locations.Commands.AcquireLocationLock;

public sealed class AcquireLocationLockHandler : IRequestHandler<AcquireLocationLockCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IModificationContext _modificationContext;

    public AcquireLocationLockHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IModificationContext modificationContext)
    {
        _dbContext           = dbContext;
        _tenantProvider      = tenantProvider;
        _modificationContext = modificationContext;
    }

    public async Task Handle(AcquireLocationLockCommand request, CancellationToken cancellationToken)
    {
        var location = await _dbContext.Locations
            .FirstOrDefaultAsync(l =>
                l.Id == request.LocationId &&
                l.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Location record not found.");

        var lockTimeout = await LockTimeoutResolver.ResolveAsync(
            _dbContext, _tenantProvider.GetAgencyId(), cancellationToken);
        location.AcquireLock(_modificationContext, lockTimeout);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
