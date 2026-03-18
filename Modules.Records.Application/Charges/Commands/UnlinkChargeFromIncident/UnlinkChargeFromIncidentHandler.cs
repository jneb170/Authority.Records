using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Charges.Commands.UnlinkChargeFromIncident;

public sealed class UnlinkChargeFromIncidentHandler : IRequestHandler<UnlinkChargeFromIncidentCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public UnlinkChargeFromIncidentHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(UnlinkChargeFromIncidentCommand request, CancellationToken cancellationToken)
    {
        var link = await _dbContext.IncidentChargeLinks.FirstOrDefaultAsync(
            l => l.IncidentId == request.IncidentId && l.ChargeId == request.ChargeId,
            cancellationToken);

        if (link is null)
            return;

        link.Unlink(_tenantProvider.GetUserId());
        _dbContext.IncidentChargeLinks.Remove(link);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
