using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Citations.Commands.UnlinkCitationFromIncident;

public sealed class UnlinkCitationFromIncidentHandler : IRequestHandler<UnlinkCitationFromIncidentCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public UnlinkCitationFromIncidentHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(UnlinkCitationFromIncidentCommand request, CancellationToken cancellationToken)
    {
        var userId = _tenantProvider.GetUserId();

        var link = await _dbContext.IncidentCitationLinks
            .FirstOrDefaultAsync(l => l.CitationId == request.CitationId && l.IncidentId == request.IncidentId, cancellationToken);

        if (link is null)
            return;

        link.Unlink(userId);
        _dbContext.IncidentCitationLinks.Remove(link);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
