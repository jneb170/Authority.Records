using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Names.Commands.ReleaseNameLock;

public sealed class ReleaseNameLockHandler : IRequestHandler<ReleaseNameLockCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IModificationContext _modificationContext;

    public ReleaseNameLockHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IModificationContext modificationContext)
    {
        _dbContext           = dbContext;
        _tenantProvider      = tenantProvider;
        _modificationContext = modificationContext;
    }

    public async Task Handle(ReleaseNameLockCommand request, CancellationToken cancellationToken)
    {
        var name = await _dbContext.Names
            .FirstOrDefaultAsync(n =>
                n.Id == request.NameId &&
                n.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Name record not found.");

        name.ReleaseLock(_modificationContext);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
