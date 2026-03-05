using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Arrests.Commands.AcquireArrestLock;

public sealed class AcquireArrestLockHandler : IRequestHandler<AcquireArrestLockCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IModificationContext _modificationContext;

    public AcquireArrestLockHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IModificationContext modificationContext)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _modificationContext = modificationContext;
    }

    public async Task Handle(AcquireArrestLockCommand request, CancellationToken cancellationToken)
    {
        var arrest = await _dbContext.Arrests
            .FirstOrDefaultAsync(a =>
                a.Id == request.ArrestId &&
                a.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Arrest not found.");

        arrest.AcquireLock(_modificationContext, TimeSpan.FromMinutes(10));

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
