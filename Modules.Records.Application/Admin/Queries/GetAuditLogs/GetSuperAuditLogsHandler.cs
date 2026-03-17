using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.ReadModels;

namespace Modules.Records.Application.Admin.Queries.GetAuditLogs;

public sealed class GetSuperAuditLogsHandler : IRequestHandler<GetSuperAuditLogsQuery, AuditLogQueryResult>
{
    private readonly IApplicationDbContext _db;
    private readonly IUserLookupService _userLookupService;

    public GetSuperAuditLogsHandler(
        IApplicationDbContext db,
        IUserLookupService userLookupService)
    {
        _db = db;
        _userLookupService = userLookupService;
    }

    public Task<AuditLogQueryResult> Handle(
        GetSuperAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<AuditLogReadModel> query = _db.AuditLogReadModels
            .AsNoTracking();

        query = request.Request.Scope switch
        {
            AuditLogScopes.System => query.Where(x => x.JurisdictionId == null),
            AuditLogScopes.Jurisdiction when request.Request.JurisdictionId.HasValue
                => query.Where(x => x.JurisdictionId == request.Request.JurisdictionId),
            AuditLogScopes.Jurisdiction
                => query.Where(x => x.JurisdictionId != null),
            _ => query
        };

        return AuditLogQueryExecutor.ExecuteAsync(_db, query, request.Request, _userLookupService, cancellationToken);
    }
}
