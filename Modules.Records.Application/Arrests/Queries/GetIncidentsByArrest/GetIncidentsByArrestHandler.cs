using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Arrests.Queries.GetIncidentsByArrest;

public sealed class GetIncidentsByArrestHandler
    : IRequestHandler<GetIncidentsByArrestQuery, IReadOnlyList<IncidentArrestLinkDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetIncidentsByArrestHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<IncidentArrestLinkDto>> Handle(
        GetIncidentsByArrestQuery request,
        CancellationToken cancellationToken)
    {
        var results = await _dbContext.IncidentArrestLinkReadModels
            .AsNoTracking()
            .Where(l => l.ArrestId == request.ArrestId)
            .ToListAsync(cancellationToken);

        return results.Select(l => new IncidentArrestLinkDto(
            l.Id,
            l.IncidentId,
            l.IncidentRecordNumber,
            l.IncidentNum,
            l.ArrestId,
            l.LinkedAtUtc)).ToList();
    }
}
