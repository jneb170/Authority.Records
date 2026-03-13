using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Locations.Commands.RestoreLocation;

public sealed class RestoreLocationHandler : IRequestHandler<RestoreLocationCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public RestoreLocationHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext      = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(RestoreLocationCommand request, CancellationToken cancellationToken)
    {
        var location = await _dbContext.Locations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(l =>
                l.Id == request.LocationId &&
                l.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Location record not found.");

        location.Restore(_tenantProvider.GetUserId());

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
