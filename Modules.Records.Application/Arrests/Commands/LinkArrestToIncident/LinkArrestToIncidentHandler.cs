using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Arrests.Commands.LinkArrestToIncident;

public sealed class LinkArrestToIncidentHandler : IRequestHandler<LinkArrestToIncidentCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public LinkArrestToIncidentHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(LinkArrestToIncidentCommand request, CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var userId = _tenantProvider.GetUserId();

        var arrest = await _dbContext.Arrests
            .FirstOrDefaultAsync(a => a.Id == request.ArrestId, cancellationToken)
            ?? throw new InvalidOperationException("Arrest not found.");

        var incident = await _dbContext.Incidents
            .FirstOrDefaultAsync(i => i.Id == request.IncidentId, cancellationToken)
            ?? throw new InvalidOperationException("Incident not found.");

        var alreadyLinked = await _dbContext.IncidentArrestLinks
            .AnyAsync(l => l.ArrestId == request.ArrestId && l.IncidentId == request.IncidentId, cancellationToken);

        if (alreadyLinked)
            return;

        var link = new IncidentArrestLink(jurisdictionId, request.IncidentId, request.ArrestId, userId);
        _dbContext.IncidentArrestLinks.Add(link);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
