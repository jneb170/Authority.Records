using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Charges.Commands.UpdateCharge;

public sealed class UpdateChargeHandler : IRequestHandler<UpdateChargeCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public UpdateChargeHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(UpdateChargeCommand request, CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var agencyId = _tenantProvider.GetAgencyId();

        var charge = await _dbContext.Charges.FirstOrDefaultAsync(
            c => c.Id == request.ChargeId &&
                 c.JurisdictionId == jurisdictionId &&
                 c.AgencyId == agencyId,
            cancellationToken)
            ?? throw new InvalidOperationException("Charge not found.");

        var duplicateExists = await _dbContext.Charges.AnyAsync(
            c => c.Id != request.ChargeId &&
                 c.JurisdictionId == jurisdictionId &&
                 c.AgencyId == agencyId &&
                 c.UcrCode == request.UcrCode &&
                 c.OffenseName == request.OffenseName,
            cancellationToken);

        if (duplicateExists)
            throw new InvalidOperationException("A charge with the same UCR code and offense name already exists.");

        charge.Update(
            request.OffenseName,
            request.UcrCategory,
            request.NibrsGroup,
            request.CrimeAgainst,
            request.UcrCode,
            request.ChargeLevel,
            request.StateClass,
            request.IsCitationEligible);

        if (request.IsActive)
            charge.Activate();
        else
            charge.Deactivate();

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
