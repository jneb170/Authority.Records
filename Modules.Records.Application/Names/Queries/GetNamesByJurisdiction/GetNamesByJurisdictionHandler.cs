using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Names.Queries.GetNamesByJurisdiction;

public sealed class GetNamesByJurisdictionHandler
    : IRequestHandler<GetNamesByJurisdictionQuery, IReadOnlyList<NameDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetNamesByJurisdictionHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext      = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<IReadOnlyList<NameDto>> Handle(
        GetNamesByJurisdictionQuery request,
        CancellationToken cancellationToken)
    {
        var results = await _dbContext.NameReadModels
            .AsNoTracking()
            .Where(n => n.JurisdictionId == _tenantProvider.GetJurisdictionId())
            .OrderBy(n => n.LastOrBusinessName)
            .ThenBy(n => n.FirstName)
            .ToListAsync(cancellationToken);

        return results.Select(r => r.ToDto()).ToList();
    }
}
