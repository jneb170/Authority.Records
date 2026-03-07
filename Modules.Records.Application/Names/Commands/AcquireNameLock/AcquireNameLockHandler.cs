using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Names.Commands.AcquireNameLock;

public sealed class AcquireNameLockHandler : IRequestHandler<AcquireNameLockCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IModificationContext _modificationContext;

    public AcquireNameLockHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IModificationContext modificationContext)
    {
        _dbContext           = dbContext;
        _tenantProvider      = tenantProvider;
        _modificationContext = modificationContext;
    }

    public async Task Handle(AcquireNameLockCommand request, CancellationToken cancellationToken)
    {
        var name = await _dbContext.Names
            .FirstOrDefaultAsync(n =>
                n.Id == request.NameId &&
                n.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Name record not found.");

        name.AcquireLock(_modificationContext, TimeSpan.FromMinutes(10));

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
