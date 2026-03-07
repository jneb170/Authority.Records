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
        var results = await _dbContext.CitationReadModels
            .AsNoTracking()
            .Where(c => c.IncidentId == request.IncidentId)
            .ToListAsync(cancellationToken);

        return results.Select(rm => new CitationDto(
            rm.Id,
            rm.JurisdictionId,
            rm.AgencyId,
            rm.IncidentId,
            rm.Description,
            rm.IssueDate,
            rm.IsIssued,
            rm.IsLocked,
            rm.LockedByUserId,
            rm.CreatedBy,
            rm.ModifiedBy,
            rm.CreatedAtUtc,
            rm.UpdatedAtUtc)).ToList();
    }
}
