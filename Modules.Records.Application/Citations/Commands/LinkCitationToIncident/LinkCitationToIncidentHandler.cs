using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Citations.Commands.LinkCitationToIncident;

public sealed class LinkCitationToIncidentHandler : IRequestHandler<LinkCitationToIncidentCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public LinkCitationToIncidentHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(LinkCitationToIncidentCommand request, CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var userId = _tenantProvider.GetUserId();

        var citation = await _dbContext.Citations
            .FirstOrDefaultAsync(c => c.Id == request.CitationId, cancellationToken)
            ?? throw new InvalidOperationException("Citation not found.");

        var incident = await _dbContext.Incidents
            .FirstOrDefaultAsync(i => i.Id == request.IncidentId, cancellationToken)
            ?? throw new InvalidOperationException("Incident not found.");

        var alreadyLinked = await _dbContext.IncidentCitationLinks
            .AnyAsync(l => l.CitationId == request.CitationId && l.IncidentId == request.IncidentId, cancellationToken);

        if (alreadyLinked)
            return;

        var link = new IncidentCitationLink(jurisdictionId, request.IncidentId, request.CitationId, userId);
        _dbContext.IncidentCitationLinks.Add(link);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
