using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Locations.Commands.ReleaseLocationLock;

public sealed class ReleaseLocationLockHandler : IRequestHandler<ReleaseLocationLockCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IModificationContext _modificationContext;

    public ReleaseLocationLockHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IModificationContext modificationContext)
    {
        _dbContext           = dbContext;
        _tenantProvider      = tenantProvider;
        _modificationContext = modificationContext;
    }

    public async Task Handle(ReleaseLocationLockCommand request, CancellationToken cancellationToken)
    {
        var location = await _dbContext.Locations
            .FirstOrDefaultAsync(l =>
                l.Id == request.LocationId &&
                l.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Location record not found.");

        location.ReleaseLock(_modificationContext);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
