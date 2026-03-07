using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Picklists.Queries.GetPicklistItemsByIds;

public sealed class GetPicklistItemsByIdsHandler
    : IRequestHandler<GetPicklistItemsByIdsQuery, Dictionary<Guid, string>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetPicklistItemsByIdsHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<Dictionary<Guid, string>> Handle(
        GetPicklistItemsByIdsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Ids.Count == 0) return [];

        var jurisdictionId = _tenantProvider.GetJurisdictionId();

        return await _dbContext.PicklistItems
            .Where(p => p.JurisdictionId == jurisdictionId && request.Ids.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Label, cancellationToken);
    }
}
