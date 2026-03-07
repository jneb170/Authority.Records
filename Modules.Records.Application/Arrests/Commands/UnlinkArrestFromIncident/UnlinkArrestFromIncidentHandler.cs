using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Arrests.Commands.UnlinkArrestFromIncident;

public sealed class UnlinkArrestFromIncidentHandler : IRequestHandler<UnlinkArrestFromIncidentCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public UnlinkArrestFromIncidentHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(UnlinkArrestFromIncidentCommand request, CancellationToken cancellationToken)
    {
        var userId = _tenantProvider.GetUserId();

        var link = await _dbContext.IncidentArrestLinks
            .FirstOrDefaultAsync(l => l.ArrestId == request.ArrestId && l.IncidentId == request.IncidentId, cancellationToken);

        if (link is null)
            return;

        link.Unlink(userId);
        _dbContext.IncidentArrestLinks.Remove(link);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
