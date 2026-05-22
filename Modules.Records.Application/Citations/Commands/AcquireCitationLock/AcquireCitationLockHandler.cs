using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Common;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Citations.Commands.AcquireCitationLock;

public sealed class AcquireCitationLockHandler : IRequestHandler<AcquireCitationLockCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IModificationContext _modificationContext;

    public AcquireCitationLockHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IModificationContext modificationContext)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _modificationContext = modificationContext;
    }

    public async Task Handle(AcquireCitationLockCommand request, CancellationToken cancellationToken)
    {
        var citation = await _dbContext.Citations
            .FirstOrDefaultAsync(c =>
                c.Id == request.CitationId &&
                c.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Citation not found.");

        var lockTimeout = await LockTimeoutResolver.ResolveAsync(
            _dbContext, _tenantProvider.GetAgencyId(), cancellationToken);
        citation.AcquireLock(_modificationContext, lockTimeout);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
