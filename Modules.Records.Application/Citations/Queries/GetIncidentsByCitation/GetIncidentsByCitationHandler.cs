using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Common.Extensions;
using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Citations.Queries.GetIncidentsByCitation;

public sealed class GetIncidentsByCitationHandler
    : IRequestHandler<GetIncidentsByCitationQuery, IReadOnlyList<IncidentCitationLinkDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetIncidentsByCitationHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<IReadOnlyList<IncidentCitationLinkDto>> Handle(
        GetIncidentsByCitationQuery request,
        CancellationToken cancellationToken)
    {
        var agencyId = _tenantProvider.GetAgencyId();
        var results = await _dbContext.IncidentCitationLinkReadModels
            .AsNoTracking()
            .Where(l => l.CitationId == request.CitationId)
            .Join(
                _dbContext.IncidentReadModels.AsNoTracking().WhereAgencyScoped(agencyId),
                link => link.IncidentId,
                incident => incident.Id,
                (link, incident) => new IncidentCitationLinkDto(
                    link.Id,
                    incident.Id,
                    incident.RecordNumber,
                    incident.IncidentNum,
                    link.CitationId,
                    link.LinkedAtUtc))
            .ToListAsync(cancellationToken);

        return results;
    }
}
