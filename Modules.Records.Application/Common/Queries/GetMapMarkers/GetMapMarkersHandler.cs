using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;

namespace Modules.Records.Application.Common.Queries.GetMapMarkers;

public sealed class GetMapMarkersHandler
    : IRequestHandler<GetMapMarkersQuery, IReadOnlyList<MapMarkerDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetMapMarkersHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<MapMarkerDto>> Handle(
        GetMapMarkersQuery request,
        CancellationToken cancellationToken)
    {
        var jurisdictionId = request.JurisdictionId;
        var since          = request.Since;

        // Load location coordinates indexed by LocationId for fast lookup
        var locationCoords = await _dbContext.LocationReadModels
            .AsNoTracking()
            .Where(l => l.JurisdictionId == jurisdictionId
                     && l.Coordinates != null && l.Coordinates != string.Empty)
            .Select(l => new { l.Id, l.Coordinates })
            .ToDictionaryAsync(l => l.Id, l => l.Coordinates, cancellationToken);

        var markers = new List<MapMarkerDto>();

        // Incidents — filter by OccurredOn; fall back to CreatedAtUtc when OccurredOn is null
        var incidents = await _dbContext.IncidentReadModels
            .AsNoTracking()
            .Where(r => r.JurisdictionId == jurisdictionId
                     && !r.IsDeleted
                     && r.LocationId != null
                     && (since == null
                         || (r.OccurredOn != null ? r.OccurredOn >= since : r.CreatedAtUtc >= since)))
            .ToListAsync(cancellationToken);

        foreach (var r in incidents)
        {
            if (r.LocationId is null) continue;
            if (!locationCoords.TryGetValue(r.LocationId.Value, out var coords)) continue;
            if (!TryParseCoords(coords, out var lat, out var lng)) continue;

            var label = string.IsNullOrWhiteSpace(r.IncidentNum) ? $"#{r.RecordNumber}" : r.IncidentNum;
            var refDate = r.OccurredOn ?? r.CreatedAtUtc;
            markers.Add(new MapMarkerDto(
                "Incident", r.Id, r.RecordNumber, label,
                $"/incidents/{r.RecordNumber}",
                lat, lng, refDate));
        }

        // Arrests — filter by ArrestedAt; fall back to CreatedAtUtc when since is null
        var arrests = await _dbContext.ArrestReadModels
            .AsNoTracking()
            .Where(r => r.JurisdictionId == jurisdictionId
                     && r.LocationId != null
                     && (since == null || r.ArrestedAt >= since))
            .ToListAsync(cancellationToken);

        foreach (var r in arrests)
        {
            if (r.LocationId is null) continue;
            if (!locationCoords.TryGetValue(r.LocationId.Value, out var coords)) continue;
            if (!TryParseCoords(coords, out var lat, out var lng)) continue;

            var label = string.IsNullOrWhiteSpace(r.ArrestNum) ? $"#{r.RecordNumber}" : r.ArrestNum;
            markers.Add(new MapMarkerDto(
                "Arrest", r.Id, r.RecordNumber, label,
                $"/arrests/{r.RecordNumber}",
                lat, lng, r.ArrestedAt));
        }

        // Citations — filter by IssueDate
        var citations = await _dbContext.CitationReadModels
            .AsNoTracking()
            .Where(r => r.JurisdictionId == jurisdictionId
                     && r.LocationId != null
                     && (since == null || r.IssueDate >= since))
            .ToListAsync(cancellationToken);

        foreach (var r in citations)
        {
            if (r.LocationId is null) continue;
            if (!locationCoords.TryGetValue(r.LocationId.Value, out var coords)) continue;
            if (!TryParseCoords(coords, out var lat, out var lng)) continue;

            var label = string.IsNullOrWhiteSpace(r.CitationNum) ? $"#{r.RecordNumber}" : r.CitationNum;
            markers.Add(new MapMarkerDto(
                "Citation", r.Id, r.RecordNumber, label,
                $"/citations/{r.RecordNumber}",
                lat, lng, r.IssueDate));
        }

        return markers;
    }

    private static bool TryParseCoords(string? coords, out double lat, out double lng)
    {
        lat = 0; lng = 0;
        if (string.IsNullOrWhiteSpace(coords)) return false;

        var parts = coords.Split(',', 2);
        if (parts.Length != 2) return false;

        return double.TryParse(parts[0].Trim(), System.Globalization.NumberStyles.Float,
                   System.Globalization.CultureInfo.InvariantCulture, out lat)
            && double.TryParse(parts[1].Trim(), System.Globalization.NumberStyles.Float,
                   System.Globalization.CultureInfo.InvariantCulture, out lng);
    }
}
