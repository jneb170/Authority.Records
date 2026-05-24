using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Common;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;

namespace Modules.Records.Application.Narratives.Commands.AcquireNarrativeLock;

public sealed class AcquireNarrativeLockHandler : IRequestHandler<AcquireNarrativeLockCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IModificationContext _modificationContext;

    public AcquireNarrativeLockHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IModificationContext modificationContext)
    {
        _dbContext           = dbContext;
        _tenantProvider      = tenantProvider;
        _modificationContext = modificationContext;
    }

    public async Task Handle(AcquireNarrativeLockCommand request, CancellationToken cancellationToken)
    {
        var narrative = await _dbContext.Narratives
            .FirstOrDefaultAsync(n =>
                n.Id == request.NarrativeId &&
                n.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Narrative record not found.");

        var lockingAgencyId = _tenantProvider.GetAgencyId();
        // Narratives use a deliberately long timeout so a long-form composer isn't timed out.
        var lockTimeout = await LockTimeoutResolver.ResolveAsync(
            _dbContext,
            lockingAgencyId,
            ConfigurationKeys.NarrativeLockTimeoutSeconds,
            ConfigurationKeys.DefaultNarrativeLockTimeoutSeconds,
            cancellationToken);

        narrative.AcquireLock(_modificationContext, lockTimeout, lockingAgencyId);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
