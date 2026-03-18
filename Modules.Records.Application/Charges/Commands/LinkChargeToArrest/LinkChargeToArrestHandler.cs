using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Charges.Commands.LinkChargeToArrest;

public sealed class LinkChargeToArrestHandler : IRequestHandler<LinkChargeToArrestCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public LinkChargeToArrestHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(LinkChargeToArrestCommand request, CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var userId = _tenantProvider.GetUserId();

        _ = await _dbContext.Arrests.FirstOrDefaultAsync(a => a.Id == request.ArrestId, cancellationToken)
            ?? throw new InvalidOperationException("Arrest not found.");

        var charge = await _dbContext.Charges.FirstOrDefaultAsync(c => c.Id == request.ChargeId, cancellationToken)
            ?? throw new InvalidOperationException("Charge not found.");

        if (!charge.IsActive)
            throw new InvalidOperationException("Only active charges can be linked.");

        var alreadyLinked = await _dbContext.ArrestChargeLinks.AnyAsync(
            l => l.ArrestId == request.ArrestId && l.ChargeId == request.ChargeId,
            cancellationToken);

        if (alreadyLinked)
            return;

        _dbContext.ArrestChargeLinks.Add(new ArrestChargeLink(jurisdictionId, request.ArrestId, request.ChargeId, userId));
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
