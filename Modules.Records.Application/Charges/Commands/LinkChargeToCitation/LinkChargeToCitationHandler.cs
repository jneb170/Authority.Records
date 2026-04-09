using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Charges.Commands.LinkChargeToCitation;

public sealed class LinkChargeToCitationHandler : IRequestHandler<LinkChargeToCitationCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public LinkChargeToCitationHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(LinkChargeToCitationCommand request, CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var userId = _tenantProvider.GetUserId();

        _ = await _dbContext.Citations.FirstOrDefaultAsync(c => c.Id == request.CitationId, cancellationToken)
            ?? throw new InvalidOperationException("Citation not found.");

        var charge = await _dbContext.Charges.FirstOrDefaultAsync(c => c.Id == request.ChargeId, cancellationToken)
            ?? throw new InvalidOperationException("Charge not found.");

        if (!charge.IsActive)
            throw new InvalidOperationException("Only active charges can be linked.");

        if (!charge.IsCitationEligible)
            throw new InvalidOperationException("This charge is not eligible for citation use.");

        var alreadyLinked = await _dbContext.CitationChargeLinks.AnyAsync(
            l => l.CitationId == request.CitationId && l.ChargeId == request.ChargeId,
            cancellationToken);

        if (alreadyLinked)
            return;

        _dbContext.CitationChargeLinks.Add(new CitationChargeLink(jurisdictionId, request.CitationId, request.ChargeId, userId));
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
