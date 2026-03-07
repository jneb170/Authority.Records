using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Citations.Queries.GetIncidentsByCitation;

public sealed class GetIncidentsByCitationHandler
    : IRequestHandler<GetIncidentsByCitationQuery, IReadOnlyList<IncidentCitationLinkDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetIncidentsByCitationHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<IncidentCitationLinkDto>> Handle(
        GetIncidentsByCitationQuery request,
        CancellationToken cancellationToken)
    {
        var results = await _dbContext.IncidentCitationLinkReadModels
            .AsNoTracking()
            .Where(l => l.CitationId == request.CitationId)
            .ToListAsync(cancellationToken);

        return results.Select(l => new IncidentCitationLinkDto(
            l.Id,
            l.IncidentId,
            l.IncidentRecordNumber,
            l.IncidentNum,
            l.CitationId,
            l.LinkedAtUtc)).ToList();
    }
}
