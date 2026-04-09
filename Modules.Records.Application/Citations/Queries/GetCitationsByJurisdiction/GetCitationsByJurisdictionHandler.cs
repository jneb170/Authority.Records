using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Common.Extensions;
using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Citations.Queries.GetCitationsByJurisdiction;

public sealed class GetCitationsByJurisdictionHandler
    : IRequestHandler<GetCitationsByJurisdictionQuery, IReadOnlyList<CitationDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetCitationsByJurisdictionHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<IReadOnlyList<CitationDto>> Handle(
        GetCitationsByJurisdictionQuery request,
        CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var results = await _dbContext.CitationReadModels
            .AsNoTracking()
            .WhereAgencyScoped(_tenantProvider.GetAgencyId())
            .Where(c => c.JurisdictionId == jurisdictionId)
            .OrderByDescending(c => c.IssueDate)
            .ToListAsync(cancellationToken);

        return await CitationDtoMapper.ToDtosAsync(results, _dbContext, cancellationToken);
    }
}
