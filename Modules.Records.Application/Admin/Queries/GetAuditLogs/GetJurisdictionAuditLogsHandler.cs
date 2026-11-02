using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Admin.Queries.GetAuditLogs;

public sealed class GetJurisdictionAuditLogsHandler : IRequestHandler<GetJurisdictionAuditLogsQuery, AuditLogQueryResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantProvider _tenantProvider;
    private readonly IUserLookupService _userLookupService;

    public GetJurisdictionAuditLogsHandler(
        IApplicationDbContext db,
        ITenantProvider tenantProvider,
        IUserLookupService userLookupService)
    {
        _db = db;
        _tenantProvider = tenantProvider;
        _userLookupService = userLookupService;
    }

    public Task<AuditLogQueryResult> Handle(
        GetJurisdictionAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();

        var scopedRequest = request.Request with
        {
            Scope = AuditLogScopes.Jurisdiction,
            JurisdictionId = jurisdictionId
        };

        // An agency admin sees their active agency's trail plus jurisdiction-wide rows (shared
        // names and locations, system work), never another agency's incidents, arrests or
        // citations (issue #118).
        var agencyId = _tenantProvider.GetAgencyId();

        var query = _db.AuditLogReadModels
            .AsNoTracking()
            .Where(x => x.JurisdictionId == jurisdictionId)
            .Where(x => x.AgencyId == null || x.AgencyId == agencyId);

        return AuditLogQueryExecutor.ExecuteAsync(_db, query, scopedRequest, _userLookupService, cancellationToken);
    }
}
