using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Picklists.Commands.SeedDefaultPicklistItems;

public sealed class SeedDefaultPicklistItemsHandler : IRequestHandler<SeedDefaultPicklistItemsCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public SeedDefaultPicklistItemsHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(SeedDefaultPicklistItemsCommand request, CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var agencyId = _tenantProvider.GetAgencyId();
        var defaults = PicklistDefaults.For(request.PicklistType);

        if (defaults.Count == 0) return;

        var existingValues = await _dbContext.PicklistItems
            .Where(p =>
                p.JurisdictionId == jurisdictionId &&
                p.AgencyId == agencyId &&
                p.PicklistType == request.PicklistType)
            .Select(p => p.Value)
            .ToListAsync(cancellationToken);

        var sortOrder = existingValues.Count;
        foreach (var (value, label) in defaults)
        {
            if (existingValues.Contains(value)) continue;

            var item = new PicklistItem(
                jurisdictionId, agencyId,
                request.PicklistType, value, label,
                sortOrder++, isSystemDefault: true);

            _dbContext.PicklistItems.Add(item);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
