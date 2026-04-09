using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Charges.Commands.DeleteCharge;

public sealed class DeleteChargeHandler : IRequestHandler<DeleteChargeCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public DeleteChargeHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(DeleteChargeCommand request, CancellationToken cancellationToken)
    {
        var charge = await _dbContext.Charges.FirstOrDefaultAsync(c => c.Id == request.ChargeId, cancellationToken)
            ?? throw new InvalidOperationException("Charge not found.");

        var isLinked = await _dbContext.IncidentChargeLinks.AnyAsync(l => l.ChargeId == request.ChargeId, cancellationToken)
            || await _dbContext.ArrestChargeLinks.AnyAsync(l => l.ChargeId == request.ChargeId, cancellationToken)
            || await _dbContext.CitationChargeLinks.AnyAsync(l => l.ChargeId == request.ChargeId, cancellationToken);

        if (isLinked)
            throw new InvalidOperationException("This charge is linked to existing records and cannot be deleted. Deactivate it instead.");

        charge.Delete(_tenantProvider.GetUserId());
        _dbContext.Charges.Remove(charge);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
