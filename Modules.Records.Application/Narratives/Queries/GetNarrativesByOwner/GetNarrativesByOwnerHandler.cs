using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Narratives.Queries.GetNarrativesByOwner;

public sealed class GetNarrativesByOwnerHandler
    : IRequestHandler<GetNarrativesByOwnerQuery, IReadOnlyList<NarrativeDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetNarrativesByOwnerHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext      = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<IReadOnlyList<NarrativeDto>> Handle(
        GetNarrativesByOwnerQuery request, CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();

        var query =
            from link in _dbContext.NarrativeLinkReadModels.AsNoTracking()
            where link.JurisdictionId == jurisdictionId
               && link.OwnerType == request.OwnerType
               && link.OwnerId == request.OwnerId
            join narrative in _dbContext.NarrativeReadModels.AsNoTracking()
                on link.NarrativeId equals narrative.Id
            orderby link.DisplayOrder
            select narrative;

        var results = await query.ToListAsync(cancellationToken);
        return results.Select(n => n.ToDto()).ToList();
    }
}
