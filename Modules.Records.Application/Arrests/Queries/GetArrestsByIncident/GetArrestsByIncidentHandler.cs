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
        var arrestIds = await _dbContext.IncidentArrestLinkReadModels
            .AsNoTracking()
            .Where(l => l.IncidentId == request.IncidentId)
            .Select(l => l.ArrestId)
            .ToListAsync(cancellationToken);

        var results = await _dbContext.ArrestReadModels
            .AsNoTracking()
            .Where(a => arrestIds.Contains(a.Id))
            .ToListAsync(cancellationToken);

        return await ArrestDtoMapper.ToDtosAsync(results, _dbContext, cancellationToken);
    }
}
