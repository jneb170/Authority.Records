using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Names.Commands.CreateName;

public sealed class CreateNameHandler : IRequestHandler<CreateNameCommand, long>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public CreateNameHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext      = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<long> Handle(CreateNameCommand request, CancellationToken cancellationToken)
    {
        var name = new Name(
            _tenantProvider.GetJurisdictionId(),
            _tenantProvider.GetAgencyId(),
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
            request.DeceasedDate);

        _dbContext.Names.Add(name);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return name.RecordNumber;
    }
}
