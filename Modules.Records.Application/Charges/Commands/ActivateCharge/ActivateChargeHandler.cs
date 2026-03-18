using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;

namespace Modules.Records.Application.Charges.Commands.ActivateCharge;

public sealed class ActivateChargeHandler : IRequestHandler<ActivateChargeCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public ActivateChargeHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(ActivateChargeCommand request, CancellationToken cancellationToken)
    {
        var charge = await _dbContext.Charges.FirstOrDefaultAsync(c => c.Id == request.ChargeId, cancellationToken)
            ?? throw new InvalidOperationException("Charge not found.");

        charge.Activate();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
