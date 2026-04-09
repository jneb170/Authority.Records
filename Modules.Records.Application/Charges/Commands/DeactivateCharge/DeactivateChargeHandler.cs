using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;

namespace Modules.Records.Application.Charges.Commands.DeactivateCharge;

public sealed class DeactivateChargeHandler : IRequestHandler<DeactivateChargeCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public DeactivateChargeHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeactivateChargeCommand request, CancellationToken cancellationToken)
    {
        var charge = await _dbContext.Charges.FirstOrDefaultAsync(c => c.Id == request.ChargeId, cancellationToken)
            ?? throw new InvalidOperationException("Charge not found.");

        charge.Deactivate();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
