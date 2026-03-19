using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Names.Commands.UpdateNameDetails;

public sealed class UpdateNameDetailsHandler : IRequestHandler<UpdateNameDetailsCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IModificationContext _modificationContext;

    public UpdateNameDetailsHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IModificationContext modificationContext)
    {
        _dbContext           = dbContext;
        _tenantProvider      = tenantProvider;
        _modificationContext = modificationContext;
    }

    public async Task Handle(UpdateNameDetailsCommand request, CancellationToken cancellationToken)
    {
        var name = await _dbContext.Names
            .FirstOrDefaultAsync(n =>
                n.Id == request.NameId &&
                n.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Name record not found.");

        name.UpdateDetails(
            request.NameType,
            request.LastOrBusinessName,
            request.FirstName,
            request.MiddleName,
            request.SexId,
            request.RaceId,
            request.DateOfBirth,
            request.DriversLicenseNumber,
            request.DriversLicenseStateId,
            request.HeightInches,
            request.WeightLbs,
            request.HairColorId,
            request.EyeColorId,
            request.SuffixId,
            request.PlaceOfBirth,
            request.FbiNumber,
            request.LocalNumber,
            request.SocialSecurityNumber,
            request.IsCitizen,
            request.DeceasedDate,
            _modificationContext,
            request.PrimaryPhone,
            request.PrimaryPhoneExtension,
            request.WorkPhone,
            request.WorkPhoneExtension,
            request.OtherPhone,
            request.OtherPhoneExtension);
        name.SetLocations(request.PrimaryLocationId, request.SecondaryLocationId, _modificationContext);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
