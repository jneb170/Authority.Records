using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Narratives.Commands.RenewNarrativeLock;

public sealed class RenewNarrativeLockHandler : IRequestHandler<RenewNarrativeLockCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IModificationContext _modificationContext;

    public RenewNarrativeLockHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IModificationContext modificationContext)
    {
        _dbContext           = dbContext;
        _tenantProvider      = tenantProvider;
        _modificationContext = modificationContext;
    }

    public async Task Handle(RenewNarrativeLockCommand request, CancellationToken cancellationToken)
    {
        var narrative = await _dbContext.Narratives
            .FirstOrDefaultAsync(n =>
                n.Id == request.NarrativeId &&
                n.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Narrative record not found.");

        narrative.RenewLock(_modificationContext);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
