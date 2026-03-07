using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Arrests.Queries.GetArrestsByIncident;

public sealed class GetArrestsByIncidentHandler
    : IRequestHandler<GetArrestsByIncidentQuery, IReadOnlyList<ArrestDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetArrestsByIncidentHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ArrestDto>> Handle(
        GetArrestsByIncidentQuery request,
        CancellationToken cancellationToken)
    {
        var results = await _dbContext.ArrestReadModels
            .AsNoTracking()
            .Where(a => a.IncidentId == request.IncidentId)
            .ToListAsync(cancellationToken);

        return results.Select(rm => new ArrestDto(
            rm.Id,
            rm.JurisdictionId,
            rm.AgencyId,
            rm.IncidentId,
            rm.SuspectName,
            rm.ArrestedAt,
            rm.Status,
            rm.IsLocked,
            rm.LockedByUserId,
            rm.CreatedBy,
            rm.ModifiedBy,
            rm.CreatedAtUtc,
            rm.UpdatedAtUtc)).ToList();
    }
}
