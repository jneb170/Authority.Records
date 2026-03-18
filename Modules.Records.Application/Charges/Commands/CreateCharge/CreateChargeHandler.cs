using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Charges.Commands.CreateCharge;

public sealed class CreateChargeHandler : IRequestHandler<CreateChargeCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public CreateChargeHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<Guid> Handle(CreateChargeCommand request, CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var agencyId = _tenantProvider.GetAgencyId();

        var exists = await _dbContext.Charges.AnyAsync(
            c => c.JurisdictionId == jurisdictionId &&
                 c.AgencyId == agencyId &&
                 c.UcrCode == request.UcrCode &&
                 c.OffenseName == request.OffenseName,
            cancellationToken);

        if (exists)
            throw new InvalidOperationException("A charge with the same UCR code and offense name already exists.");

        var charge = new Charge(
            jurisdictionId,
            agencyId,
            request.OffenseName,
            request.UcrCategory,
            request.NibrsGroup,
            request.CrimeAgainst,
            request.UcrCode,
            request.ChargeLevel,
            request.StateClass,
            request.IsCitationEligible);

        if (!request.IsActive)
            charge.Deactivate();

        _dbContext.Charges.Add(charge);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return charge.Id;
    }
}
