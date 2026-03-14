using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Locations.Commands.UpdateLocationDetails;

public sealed class UpdateLocationDetailsHandler : IRequestHandler<UpdateLocationDetailsCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IModificationContext _modificationContext;

    public UpdateLocationDetailsHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IModificationContext modificationContext)
    {
        _dbContext           = dbContext;
        _tenantProvider      = tenantProvider;
        _modificationContext = modificationContext;
    }

    public async Task Handle(UpdateLocationDetailsCommand request, CancellationToken cancellationToken)
    {
        var location = await _dbContext.Locations
            .FirstOrDefaultAsync(l =>
                l.Id == request.LocationId &&
                l.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Location record not found.");

        location.UpdateDetails(
            request.StreetAddress,
            request.City,
            request.StreetNumber,
            request.PreDirectionId,
            request.StreetTypeId,
            request.PostDirectionId,
            request.StateId,
            request.CountryId,
            request.Zip,
            request.AptSuite,
            request.Coordinates,
            request.CommonPlaceName,
            request.Comments,
            request.Address,
            _modificationContext);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
