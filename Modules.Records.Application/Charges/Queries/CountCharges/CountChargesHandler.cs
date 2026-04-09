using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Charges.Queries.CountCharges;

public sealed class CountChargesHandler : IRequestHandler<CountChargesQuery, int>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public CountChargesHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<int> Handle(CountChargesQuery request, CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var agencyId = _tenantProvider.GetAgencyId();

        var query = _dbContext.Charges
            .AsNoTracking()
            .Where(c => c.JurisdictionId == jurisdictionId && c.AgencyId == agencyId);

        if (!request.IncludeInactive)
            query = query.Where(c => c.IsActive);

        return await query.CountAsync(cancellationToken);
    }
}
