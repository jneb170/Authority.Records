using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Factories;

namespace Modules.Records.Application.Incidents.Commands.CreateIncident;

public sealed class CreateIncidentHandler : IRequestHandler<CreateIncidentCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IncidentFactory _factory;

    public CreateIncidentHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IncidentFactory factory)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _factory = factory;
    }

    public async Task<Guid> Handle(CreateIncidentCommand request, CancellationToken cancellationToken)
    {
        var incident = _factory.Create(new CreateIncidentRequest
        {
            JurisdictionId = _tenantProvider.GetJurisdictionId(),
            AgencyId       = request.AgencyId,
            Details        = request.Details,
        });

        _dbContext.Incidents.Add(incident);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return incident.Id;
    }
}
