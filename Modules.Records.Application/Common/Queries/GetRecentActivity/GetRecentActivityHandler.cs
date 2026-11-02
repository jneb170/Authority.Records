using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Common.Extensions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Common.Queries.GetRecentActivity;

public sealed class GetRecentActivityHandler
    : IRequestHandler<GetRecentActivityQuery, IReadOnlyList<RecentActivityDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetRecentActivityHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<IReadOnlyList<RecentActivityDto>> Handle(
        GetRecentActivityQuery request,
        CancellationToken cancellationToken)
    {
        var userId         = request.UserId;
        var allUsers       = request.IncludeAllUsers;
        var jurisdictionId = request.JurisdictionId;
        var agencyId       = _tenantProvider.GetAgencyId();

        // EF Core DbContext is not thread-safe — execute queries sequentially.
        var incidents = await _dbContext.IncidentReadModels
            .AsNoTracking()
            .WhereAgencyScoped(agencyId)
            .Where(r => r.JurisdictionId == jurisdictionId
                     && !r.IsDeleted
                     && (allUsers || r.CreatedBy == userId || r.ModifiedBy == userId))
            .OrderByDescending(r => r.UpdatedAtUtc)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        var arrests = await _dbContext.ArrestReadModels
            .AsNoTracking()
            .WhereAgencyScoped(agencyId)
            .Where(r => r.JurisdictionId == jurisdictionId
                     && (allUsers || r.CreatedBy == userId || r.ModifiedBy == userId))
            .OrderByDescending(r => r.UpdatedAtUtc)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        var citations = await _dbContext.CitationReadModels
            .AsNoTracking()
            .WhereAgencyScoped(agencyId)
            .Where(r => r.JurisdictionId == jurisdictionId
                     && (allUsers || r.CreatedBy == userId || r.ModifiedBy == userId))
            .OrderByDescending(r => r.UpdatedAtUtc)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        var names = await _dbContext.NameReadModels
            .AsNoTracking()
            .Where(r => r.JurisdictionId == jurisdictionId
                     && (allUsers || r.CreatedBy == userId || r.ModifiedBy == userId))
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
                ActivityLabel(r.ModifiedBy, r.CreatedAtUtc, r.UpdatedAtUtc),
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
                ActivityLabel(r.ModifiedBy, r.CreatedAtUtc, r.UpdatedAtUtc),
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
                ActivityLabel(r.ModifiedBy, r.CreatedAtUtc, r.UpdatedAtUtc),
                r.UpdatedAtUtc));
        }

        foreach (var r in names)
        {
            var identifier = BuildNameIdentifier(r.NameType, r.FirstName, r.LastOrBusinessName, r.RecordNumber);
            results.Add(new RecentActivityDto(
                "Name", r.Id, identifier,
                $"/names/{r.RecordNumber}",
                null,
                ActivityLabel(r.ModifiedBy, r.CreatedAtUtc, r.UpdatedAtUtc),
                r.UpdatedAtUtc));
        }

        return results
            .OrderByDescending(r => r.LastActivityAt)
            .Take(request.Take)
            .ToList();
    }

    /// <summary>
    /// "Modified" when the record was changed after it was created (by anyone), otherwise "Created". It used
    /// to say "Modified" only when the viewer made the change, so the agency-wide feed a new user sees
    /// labelled everything "Created" (issue #99). The one-minute grace covers the save that immediately
    /// follows a create on the create pages.
    /// </summary>
    internal static string ActivityLabel(Guid? modifiedBy, DateTime createdAtUtc, DateTime updatedAtUtc)
        => modifiedBy.HasValue && updatedAtUtc > createdAtUtc.AddMinutes(1) ? "Modified" : "Created";

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
