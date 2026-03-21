using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Common.Extensions;
using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Citations.Queries.GetCitationsByIncident;

public sealed class GetCitationsByIncidentHandler
    : IRequestHandler<GetCitationsByIncidentQuery, IReadOnlyList<CitationDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetCitationsByIncidentHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<IReadOnlyList<CitationDto>> Handle(
        GetCitationsByIncidentQuery request,
        CancellationToken cancellationToken)
    {
        var citationIds = await _dbContext.IncidentCitationLinkReadModels
            .AsNoTracking()
            .Where(l => l.IncidentId == request.IncidentId)
            .Select(l => l.CitationId)
            .ToListAsync(cancellationToken);

        var results = await _dbContext.CitationReadModels
            .AsNoTracking()
            .WhereAgencyScoped(_tenantProvider.GetAgencyId())
            .Where(c => citationIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        return await CitationDtoMapper.ToDtosAsync(results, _dbContext, cancellationToken);
    }
}
