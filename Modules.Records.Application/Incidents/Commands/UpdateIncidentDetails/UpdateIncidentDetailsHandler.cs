using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Incidents.Commands.UpdateIncidentDetails;

public sealed class UpdateIncidentDetailsHandler : IRequestHandler<UpdateIncidentDetailsCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IModificationContext _modificationContext;

    public UpdateIncidentDetailsHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IModificationContext modificationContext)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _modificationContext = modificationContext;
    }

    public async Task Handle(UpdateIncidentDetailsCommand request, CancellationToken cancellationToken)
    {
        var incident = await _dbContext.Incidents
            .FirstOrDefaultAsync(i =>
                i.Id == request.IncidentId &&
                i.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Incident not found.");

        incident.UpdateDetails(request.Details, _modificationContext);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
