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

        var query = _db.AuditLogReadModels
            .AsNoTracking()
            .Where(x => x.JurisdictionId == jurisdictionId);

        return AuditLogQueryExecutor.ExecuteAsync(_db, query, scopedRequest, _userLookupService, cancellationToken);
    }
}
