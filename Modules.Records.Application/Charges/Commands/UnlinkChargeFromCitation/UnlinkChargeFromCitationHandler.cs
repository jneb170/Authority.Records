using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Charges.Commands.UnlinkChargeFromCitation;

public sealed class UnlinkChargeFromCitationHandler : IRequestHandler<UnlinkChargeFromCitationCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public UnlinkChargeFromCitationHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(UnlinkChargeFromCitationCommand request, CancellationToken cancellationToken)
    {
        var link = await _dbContext.CitationChargeLinks.FirstOrDefaultAsync(
            l => l.CitationId == request.CitationId && l.ChargeId == request.ChargeId,
            cancellationToken);

        if (link is null)
            return;

        link.Unlink(_tenantProvider.GetUserId());
        _dbContext.CitationChargeLinks.Remove(link);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
