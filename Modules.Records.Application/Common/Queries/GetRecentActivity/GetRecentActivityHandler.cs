using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;

namespace Modules.Records.Application.Common.Queries.GetRecentActivity;

public sealed class GetRecentActivityHandler
    : IRequestHandler<GetRecentActivityQuery, IReadOnlyList<RecentActivityDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetRecentActivityHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<RecentActivityDto>> Handle(
        GetRecentActivityQuery request,
        CancellationToken cancellationToken)
    {
        var userId         = request.UserId;
        var jurisdictionId = request.JurisdictionId;

        // EF Core DbContext is not thread-safe — execute queries sequentially.
        var incidents = await _dbContext.IncidentReadModels
            .AsNoTracking()
            .Where(r => r.JurisdictionId == jurisdictionId
                     && !r.IsDeleted
                     && (r.CreatedBy == userId || r.ModifiedBy == userId))
            .OrderByDescending(r => r.UpdatedAtUtc)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        var arrests = await _dbContext.ArrestReadModels
            .AsNoTracking()
            .Where(r => r.JurisdictionId == jurisdictionId
                     && (r.CreatedBy == userId || r.ModifiedBy == userId))
            .OrderByDescending(r => r.UpdatedAtUtc)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        var citations = await _dbContext.CitationReadModels
            .AsNoTracking()
            .Where(r => r.JurisdictionId == jurisdictionId
                     && (r.CreatedBy == userId || r.ModifiedBy == userId))
            .OrderByDescending(r => r.UpdatedAtUtc)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        var names = await _dbContext.NameReadModels
            .AsNoTracking()
            .Where(r => r.JurisdictionId == jurisdictionId
                     && (r.CreatedBy == userId || r.ModifiedBy == userId))
            .OrderByDescending(r => r.UpdatedAtUtc)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        var results = new List<RecentActivityDto>();

        foreach (var r in incidents)
        {
            var identifier = string.IsNullOrWhiteSpace(r.IncidentNum)
                ? $"#{r.RecordNumber}"
                : r.IncidentNum;
            results.Add(new RecentActivityDto(
                "Incident", r.Id, identifier,
                $"/incidents/{r.RecordNumber}",
                r.Status,
                r.ModifiedBy == userId ? "Modified" : "Created",
                r.UpdatedAtUtc));
        }

        foreach (var r in arrests)
        {
            var identifier = string.IsNullOrWhiteSpace(r.ArrestNum)
                ? $"#{r.RecordNumber}"
                : r.ArrestNum;
            results.Add(new RecentActivityDto(
                "Arrest", r.Id, identifier,
                $"/arrests/{r.RecordNumber}",
                r.Status,
                r.ModifiedBy == userId ? "Modified" : "Created",
                r.UpdatedAtUtc));
        }

        foreach (var r in citations)
        {
            var identifier = string.IsNullOrWhiteSpace(r.CitationNum)
                ? $"#{r.RecordNumber}"
                : r.CitationNum;
            results.Add(new RecentActivityDto(
                "Citation", r.Id, identifier,
                $"/citations/{r.RecordNumber}",
                r.IsIssued ? "Issued" : "Draft",
                r.ModifiedBy == userId ? "Modified" : "Created",
                r.UpdatedAtUtc));
        }

        foreach (var r in names)
        {
            var identifier = BuildNameIdentifier(r.NameType, r.FirstName, r.LastOrBusinessName, r.RecordNumber);
            results.Add(new RecentActivityDto(
                "Name", r.Id, identifier,
                $"/names/{r.RecordNumber}",
                null,
                r.ModifiedBy == userId ? "Modified" : "Created",
                r.UpdatedAtUtc));
        }

        return results
            .OrderByDescending(r => r.LastActivityAt)
            .Take(request.Take)
            .ToList();
    }

    private static string BuildNameIdentifier(string nameType, string? firstName, string lastOrBusiness, long recordNumber)
    {
        if (nameType.Equals("Business", StringComparison.OrdinalIgnoreCase))
            return string.IsNullOrWhiteSpace(lastOrBusiness) ? $"#{recordNumber}" : lastOrBusiness;

        // Person
        if (string.IsNullOrWhiteSpace(lastOrBusiness))
            return $"#{recordNumber}";

        return string.IsNullOrWhiteSpace(firstName)
            ? lastOrBusiness
            : $"{lastOrBusiness}, {firstName}";
    }
}
