using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Citations.Queries.GetCitationsByIncident;

public sealed class GetCitationsByIncidentHandler
    : IRequestHandler<GetCitationsByIncidentQuery, IReadOnlyList<CitationDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCitationsByIncidentHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
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
            .Where(c => citationIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        return results.Select(rm => rm.ToDto()).ToList();
    }
}
