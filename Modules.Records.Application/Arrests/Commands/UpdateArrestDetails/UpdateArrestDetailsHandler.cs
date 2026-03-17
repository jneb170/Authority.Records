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
        var jurisdictionId = _tenantProvider.GetJurisdictionId();

        var arrest = await _dbContext.Arrests
            .FirstOrDefaultAsync(a =>
                a.Id == request.ArrestId &&
                a.JurisdictionId == jurisdictionId,
                cancellationToken)
            ?? throw new InvalidOperationException("Arrest not found.");

        var nameExists = await _dbContext.Names
            .AsNoTracking()
            .AnyAsync(n => n.Id == request.NameId && n.JurisdictionId == jurisdictionId, cancellationToken);

        if (!nameExists)
            throw new InvalidOperationException("Linked name not found.");

        if (request.PrimaryIncidentId.HasValue)
        {
            var incidentExists = await _dbContext.Incidents
                .AsNoTracking()
                .AnyAsync(i => i.Id == request.PrimaryIncidentId.Value && i.JurisdictionId == jurisdictionId, cancellationToken);

            if (!incidentExists)
                throw new InvalidOperationException("Primary incident not found.");
        }

        arrest.UpdateDetails(request.NameId, request.ArrestedAt, request.ArrestTypeId, request.ArrestNum, request.PrimaryIncidentId, _modificationContext);
        arrest.SetLocation(request.LocationId, _modificationContext);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
