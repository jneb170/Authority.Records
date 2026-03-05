using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Factories;

namespace Modules.Records.Application.Arrests.Commands.CreateArrest;

public sealed class CreateArrestHandler : IRequestHandler<CreateArrestCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IModificationContext _modificationContext;
    private readonly ArrestFactory _factory;

    public CreateArrestHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IModificationContext modificationContext,
        ArrestFactory factory)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _modificationContext = modificationContext;
        _factory = factory;
    }

    public async Task<Guid> Handle(CreateArrestCommand request, CancellationToken cancellationToken)
    {
        var incident = await _dbContext.Incidents
            .FirstOrDefaultAsync(i =>
                i.Id == request.IncidentId &&
                i.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Incident not found.");

        var arrest = _factory.Create(
            _tenantProvider.GetJurisdictionId(),
            incident.AgencyId,
            incident.Id,
            request.SuspectName,
            request.ArrestedAt);

        incident.AddArrest(arrest, _modificationContext);

        _dbContext.Arrests.Add(arrest);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return arrest.Id;
    }
}
