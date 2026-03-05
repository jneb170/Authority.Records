using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Arrests.Commands.OpenArrest;

public sealed class OpenArrestHandler : IRequestHandler<OpenArrestCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IModificationContext _modificationContext;
    private readonly ILifecyclePolicy<Arrest> _lifecyclePolicy;

    public OpenArrestHandler(
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

    public async Task Handle(OpenArrestCommand request, CancellationToken cancellationToken)
    {
        var arrest = await _dbContext.Arrests
            .FirstOrDefaultAsync(a =>
                a.Id == request.ArrestId &&
                a.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Arrest not found.");

        arrest.Open(_modificationContext, _lifecyclePolicy);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
