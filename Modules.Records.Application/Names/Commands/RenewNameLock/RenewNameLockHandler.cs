using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Names.Commands.RenewNameLock;

public sealed class RenewNameLockHandler : IRequestHandler<RenewNameLockCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IModificationContext _modificationContext;

    public RenewNameLockHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IModificationContext modificationContext)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _modificationContext = modificationContext;
    }

    public async Task Handle(RenewNameLockCommand request, CancellationToken cancellationToken)
    {
        var name = await _dbContext.Names
            .FirstOrDefaultAsync(n =>
                n.Id == request.NameId &&
                n.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Name record not found.");

        name.RenewLock(_modificationContext);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
