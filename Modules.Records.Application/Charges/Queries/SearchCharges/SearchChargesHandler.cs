using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Charges.Queries.SearchCharges;

public sealed class SearchChargesHandler : IRequestHandler<SearchChargesQuery, IReadOnlyList<ChargeDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public SearchChargesHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<IReadOnlyList<ChargeDto>> Handle(SearchChargesQuery request, CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var agencyId = _tenantProvider.GetAgencyId();

        // Charges are intentionally queried from the write model today.
        // They currently behave as reference/catalog data rather than read-model-driven workflow records.
        var query = _dbContext.Charges
            .AsNoTracking()
            .Where(c => c.JurisdictionId == jurisdictionId && c.AgencyId == agencyId);

        if (!request.IncludeInactive)
            query = query.Where(c => c.IsActive);

        if (request.CitationEligibleOnly)
            query = query.Where(c => c.IsCitationEligible);

        if (!string.IsNullOrWhiteSpace(request.Term))
        {
            var pattern = $"%{request.Term.Trim()}%";
            query = query.Where(c =>
                EF.Functions.Like(c.OffenseName, pattern) ||
                EF.Functions.Like(c.UcrCode, pattern) ||
                EF.Functions.Like(c.ChargeLevel, pattern) ||
                EF.Functions.Like(c.CrimeAgainst, pattern));
        }

        var charges = await query
            .OrderBy(c => c.OffenseName)
            .ThenBy(c => c.UcrCode)
            .Take(100)
            .ToListAsync(cancellationToken);

        return charges.Select(c => c.ToDto()).ToList();
    }
}
