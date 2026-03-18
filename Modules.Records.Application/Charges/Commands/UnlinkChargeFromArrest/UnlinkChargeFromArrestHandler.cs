using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Charges.Commands.UnlinkChargeFromArrest;

public sealed class UnlinkChargeFromArrestHandler : IRequestHandler<UnlinkChargeFromArrestCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public UnlinkChargeFromArrestHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(UnlinkChargeFromArrestCommand request, CancellationToken cancellationToken)
    {
        var link = await _dbContext.ArrestChargeLinks.FirstOrDefaultAsync(
            l => l.ArrestId == request.ArrestId && l.ChargeId == request.ChargeId,
            cancellationToken);

        if (link is null)
            return;

        link.Unlink(_tenantProvider.GetUserId());
        _dbContext.ArrestChargeLinks.Remove(link);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
