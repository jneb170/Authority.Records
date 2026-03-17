using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Incidents.Queries.SearchIncidents;

public sealed class SearchIncidentsHandler : IRequestHandler<SearchIncidentsQuery, IReadOnlyList<IncidentDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public SearchIncidentsHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<IReadOnlyList<IncidentDto>> Handle(
        SearchIncidentsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.IncidentReadModels
            .AsNoTracking()
            .Where(i => i.JurisdictionId == _tenantProvider.GetJurisdictionId() && !i.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Term))
        {
            var term = request.Term.Trim();
            query = query.Where(i =>
                i.RecordNumber.ToString().Contains(term) ||
                (i.IncidentNum != null && i.IncidentNum.Contains(term)) ||
                (i.Description != null && i.Description.Contains(term)) ||
                (i.CFSNum != null && i.CFSNum.Contains(term)) ||
                (i.LocalNum != null && i.LocalNum.Contains(term)));
        }

        var results = await query
            .OrderByDescending(i => i.RecordNumber)
            .ToListAsync(cancellationToken);

        return results.Select(r => r.ToDto()).ToList();
    }
}
