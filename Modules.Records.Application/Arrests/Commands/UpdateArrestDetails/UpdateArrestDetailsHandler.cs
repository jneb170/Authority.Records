using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Arrests.Commands.UpdateArrestDetails;

public sealed class UpdateArrestDetailsHandler : IRequestHandler<UpdateArrestDetailsCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IModificationContext _modificationContext;

    public UpdateArrestDetailsHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IModificationContext modificationContext)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _modificationContext = modificationContext;
    }

    public async Task Handle(UpdateArrestDetailsCommand request, CancellationToken cancellationToken)
    {
        var arrest = await _dbContext.Arrests
            .FirstOrDefaultAsync(a =>
                a.Id == request.ArrestId &&
                a.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Arrest not found.");

        arrest.UpdateDetails(request.SuspectName, request.ArrestedAt, request.ArrestTypeId, request.ArrestNum, _modificationContext);
        arrest.SetLocation(request.LocationId, _modificationContext);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
