using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Citations.Commands.CreateCitation;

public sealed class CreateCitationHandler : IRequestHandler<CreateCitationCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IModificationContext _modificationContext;

    public CreateCitationHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IModificationContext modificationContext)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _modificationContext = modificationContext;
    }

    public async Task<Guid> Handle(CreateCitationCommand request, CancellationToken cancellationToken)
    {
        var incident = await _dbContext.Incidents
            .FirstOrDefaultAsync(i =>
                i.Id == request.IncidentId &&
                i.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Incident not found.");

        var citation = new Citation(
            _tenantProvider.GetJurisdictionId(),
            incident.AgencyId,
            request.Description,
            request.IssueDate);

        incident.AddCitation(citation, _modificationContext);

        _dbContext.Citations.Add(citation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return citation.Id;
    }
}
