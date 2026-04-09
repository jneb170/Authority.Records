using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Charges.Commands.LinkChargeToIncident;

public sealed class LinkChargeToIncidentHandler : IRequestHandler<LinkChargeToIncidentCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public LinkChargeToIncidentHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(LinkChargeToIncidentCommand request, CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var userId = _tenantProvider.GetUserId();

        _ = await _dbContext.Incidents.FirstOrDefaultAsync(i => i.Id == request.IncidentId, cancellationToken)
            ?? throw new InvalidOperationException("Incident not found.");

        var charge = await _dbContext.Charges.FirstOrDefaultAsync(c => c.Id == request.ChargeId, cancellationToken)
            ?? throw new InvalidOperationException("Charge not found.");

        if (!charge.IsActive)
            throw new InvalidOperationException("Only active charges can be linked.");

        var alreadyLinked = await _dbContext.IncidentChargeLinks.AnyAsync(
            l => l.IncidentId == request.IncidentId && l.ChargeId == request.ChargeId,
            cancellationToken);

        if (alreadyLinked)
            return;

        _dbContext.IncidentChargeLinks.Add(new IncidentChargeLink(jurisdictionId, request.IncidentId, request.ChargeId, userId));
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
