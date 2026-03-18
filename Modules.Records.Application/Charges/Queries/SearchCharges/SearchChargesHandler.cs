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

        var query = _dbContext.Charges
            .AsNoTracking()
            .Where(c => c.JurisdictionId == jurisdictionId && c.AgencyId == agencyId);

        if (!request.IncludeInactive)
            query = query.Where(c => c.IsActive);

        if (request.CitationEligibleOnly)
            query = query.Where(c => c.IsCitationEligible);

        if (!string.IsNullOrWhiteSpace(request.Term))
        {
            var term = request.Term.Trim().ToLower();
            query = query.Where(c =>
                c.OffenseName.ToLower().Contains(term) ||
                c.UcrCode.ToLower().Contains(term) ||
                c.ChargeLevel.ToLower().Contains(term) ||
                c.CrimeAgainst.ToLower().Contains(term));
        }

        var charges = await query
            .OrderBy(c => c.OffenseName)
            .ThenBy(c => c.UcrCode)
            .Take(100)
            .ToListAsync(cancellationToken);

        return charges.Select(c => c.ToDto()).ToList();
    }
}
