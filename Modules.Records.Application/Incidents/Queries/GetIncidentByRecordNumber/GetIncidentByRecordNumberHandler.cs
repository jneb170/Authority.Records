using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Common.Extensions;
using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Incidents.Queries.GetIncidentByRecordNumber;

public sealed class GetIncidentByRecordNumberHandler : IRequestHandler<GetIncidentByRecordNumberQuery, IncidentDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetIncidentByRecordNumberHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<IncidentDto?> Handle(GetIncidentByRecordNumberQuery request, CancellationToken cancellationToken)
    {
        var rm = await _dbContext.IncidentReadModels
            .AsNoTracking()
            .WhereAgencyScoped(_tenantProvider.GetAgencyId())
            .FirstOrDefaultAsync(i => i.RecordNumber == request.RecordNumber, cancellationToken);

        return rm?.ToDto();
    }
}
