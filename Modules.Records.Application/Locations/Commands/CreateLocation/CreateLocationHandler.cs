using MediatR;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Locations.Commands.CreateLocation;

public sealed class CreateLocationHandler : IRequestHandler<CreateLocationCommand, long>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public CreateLocationHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext      = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<long> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
    {
        var location = new Location(
            jurisdictionId: _tenantProvider.GetJurisdictionId(),
            streetAddress:  request.StreetAddress,
            city:           request.City,
            streetNumber:   request.StreetNumber,
            preDirectionId: request.PreDirectionId,
            streetTypeId:   request.StreetTypeId,
            postDirectionId: request.PostDirectionId,
            stateId:        request.StateId,
            countryId:      request.CountryId,
            zip:            request.Zip,
            aptSuite:       request.AptSuite,
            coordinates:    request.Coordinates,
            commonPlaceName: request.CommonPlaceName,
            comments:       request.Comments);

        _dbContext.Locations.Add(location);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return location.RecordNumber;
    }
}
