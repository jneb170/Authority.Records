using Modules.Records.Application.Admin.Commands.RebuildReadModels;
using Modules.Records.Application.Admin.Queries.GetAuditLogs;

namespace Modules.Records.UI.Services;

public interface IAdminService
{
    Task<RebuildReadModelsResult> RebuildReadModelsAsync();
    Task<AuditLogQueryResult> GetJurisdictionAuditLogsAsync(AuditLogSearchRequest request);
    Task<AuditLogQueryResult> GetSuperAuditLogsAsync(AuditLogSearchRequest request);
}
