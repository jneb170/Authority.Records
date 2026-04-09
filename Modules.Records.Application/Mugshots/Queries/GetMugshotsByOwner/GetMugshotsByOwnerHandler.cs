using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Mugshots.Queries.GetMugshotsByOwner;

public sealed class GetMugshotsByOwnerHandler : IRequestHandler<GetMugshotsByOwnerQuery, IReadOnlyList<MugshotDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetMugshotsByOwnerHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<IReadOnlyList<MugshotDto>> Handle(GetMugshotsByOwnerQuery request, CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();

        var links = await _dbContext.MugshotLinkReadModels
            .AsNoTracking()
            .Where(l => l.JurisdictionId == jurisdictionId && l.OwnerType == request.OwnerType && l.OwnerId == request.OwnerId)
            .OrderByDescending(l => l.IsPrimary)
            .ThenBy(l => l.DisplayOrder)
            .ThenBy(l => l.LinkedAtUtc)
            .ToListAsync(cancellationToken);

        if (links.Count == 0)
        {
            return [];
        }

        var mugshotIds = links.Select(l => l.MugshotId).Distinct().ToList();
        var mugshots = await _dbContext.MugshotReadModels
            .AsNoTracking()
            .Where(m => mugshotIds.Contains(m.Id) && m.JurisdictionId == jurisdictionId)
            .ToDictionaryAsync(m => m.Id, cancellationToken);

        return links
            .Where(link => mugshots.ContainsKey(link.MugshotId))
            .Select(link => mugshots[link.MugshotId].ToDto(link))
            .ToList();
    }
}
