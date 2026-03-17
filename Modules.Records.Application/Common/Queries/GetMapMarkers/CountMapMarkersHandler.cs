using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;

namespace Modules.Records.Application.Common.Queries.GetMapMarkers;

public sealed class CountMapMarkersHandler
    : IRequestHandler<CountMapMarkersQuery, int>
{
    private readonly IApplicationDbContext _dbContext;

    public CountMapMarkersHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> Handle(
        CountMapMarkersQuery request,
        CancellationToken cancellationToken)
    {
        var jurisdictionId = request.JurisdictionId;
        var since          = request.Since;

        // Collect location IDs that have parseable coordinates for the jurisdiction.
        // This mirrors the approach in GetMapMarkersHandler so both queries agree on
        // which markers are valid, without pulling full coordinate strings or record data.
        var validLocationIds = await _dbContext.LocationReadModels
            .AsNoTracking()
            .Where(l => l.JurisdictionId == jurisdictionId
                     && l.Coordinates != null && l.Coordinates != string.Empty)
            .Select(l => l.Id)
            .ToListAsync(cancellationToken);

        if (validLocationIds.Count == 0)
            return 0;

        var incidentCount = await _dbContext.IncidentReadModels
            .AsNoTracking()
            .Where(r => r.JurisdictionId == jurisdictionId
                     && !r.IsDeleted
                     && r.LocationId != null
                     && validLocationIds.Contains(r.LocationId.Value)
                     && (since == null
                         || (r.OccurredOn != null ? r.OccurredOn >= since : r.CreatedAtUtc >= since)))
            .CountAsync(cancellationToken);

        var arrestCount = await _dbContext.ArrestReadModels
            .AsNoTracking()
            .Where(r => r.JurisdictionId == jurisdictionId
                     && r.LocationId != null
                     && validLocationIds.Contains(r.LocationId.Value)
                     && (since == null || r.ArrestedAt >= since))
            .CountAsync(cancellationToken);

        var citationCount = await _dbContext.CitationReadModels
            .AsNoTracking()
            .Where(r => r.JurisdictionId == jurisdictionId
                     && r.LocationId != null
                     && validLocationIds.Contains(r.LocationId.Value)
                     && (since == null || r.IssueDate >= since))
            .CountAsync(cancellationToken);

        return incidentCount + arrestCount + citationCount;
    }
}
