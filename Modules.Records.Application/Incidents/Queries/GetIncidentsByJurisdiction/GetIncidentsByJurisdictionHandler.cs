using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Incidents.Queries.GetIncidentsByJurisdiction;

public sealed class GetIncidentsByJurisdictionHandler
    : IRequestHandler<GetIncidentsByJurisdictionQuery, IReadOnlyList<IncidentDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetIncidentsByJurisdictionHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<IncidentDto>> Handle(
        GetIncidentsByJurisdictionQuery request,
        CancellationToken cancellationToken)
    {
        return await _dbContext.IncidentReadModels
            .AsNoTracking()
            .Where(i => i.JurisdictionId == request.JurisdictionId && !i.IsDeleted)
            .OrderByDescending(i => i.UpdatedAtUtc)
            .Select(rm => new IncidentDto(
                rm.Id,
                rm.JurisdictionId,
                rm.AgencyId,
                rm.Description,
                rm.Status,
                rm.IsDeleted,
                rm.IsLocked,
                rm.LockedByUserId,
                rm.ArrestCount,
                rm.CreatedAtUtc,
                rm.UpdatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
