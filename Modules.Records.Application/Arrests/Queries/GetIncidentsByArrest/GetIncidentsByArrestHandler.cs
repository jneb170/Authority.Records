using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Common.Extensions;
using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Arrests.Queries.GetIncidentsByArrest;

public sealed class GetIncidentsByArrestHandler
    : IRequestHandler<GetIncidentsByArrestQuery, IReadOnlyList<IncidentArrestLinkDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetIncidentsByArrestHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<IReadOnlyList<IncidentArrestLinkDto>> Handle(
        GetIncidentsByArrestQuery request,
        CancellationToken cancellationToken)
    {
        var agencyId = _tenantProvider.GetAgencyId();
        var results = await _dbContext.IncidentArrestLinkReadModels
            .AsNoTracking()
            .Where(l => l.ArrestId == request.ArrestId)
            .Join(
                _dbContext.IncidentReadModels.AsNoTracking().WhereAgencyScoped(agencyId),
                link => link.IncidentId,
                incident => incident.Id,
                (link, incident) => new IncidentArrestLinkDto(
                    link.Id,
                    incident.Id,
                    incident.RecordNumber,
                    incident.IncidentNum,
                    link.ArrestId,
                    link.LinkedAtUtc))
            .ToListAsync(cancellationToken);

        return results;
    }
}
