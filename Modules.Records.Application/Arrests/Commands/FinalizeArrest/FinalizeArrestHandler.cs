using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Arrests.Commands.FinalizeArrest;

public sealed class FinalizeArrestHandler : IRequestHandler<FinalizeArrestCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public FinalizeArrestHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(FinalizeArrestCommand request, CancellationToken cancellationToken)
    {
        var arrest = await _dbContext.Arrests
            .FirstOrDefaultAsync(a =>
                a.Id == request.ArrestId &&
                a.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Arrest not found.");

        arrest.Finalize();

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
