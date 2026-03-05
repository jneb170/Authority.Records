using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Arrests.Commands.CloseArrest;

public sealed class CloseArrestHandler : IRequestHandler<CloseArrestCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IModificationContext _modificationContext;
    private readonly ILifecyclePolicy<Arrest> _lifecyclePolicy;

    public CloseArrestHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IModificationContext modificationContext,
        ILifecyclePolicy<Arrest> lifecyclePolicy)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _modificationContext = modificationContext;
        _lifecyclePolicy = lifecyclePolicy;
    }

    public async Task Handle(CloseArrestCommand request, CancellationToken cancellationToken)
    {
        var arrest = await _dbContext.Arrests
            .FirstOrDefaultAsync(a =>
                a.Id == request.ArrestId &&
                a.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Arrest not found.");

        arrest.Close(_modificationContext, _lifecyclePolicy, request.Force);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
