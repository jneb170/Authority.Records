using Modules.Records.Application.Admin.Commands.RebuildReadModels;
using Modules.Records.Application.Admin.Queries.GetAuditLogs;

namespace Modules.Records.UI.Services;

public interface IAdminService
{
    Task<RebuildReadModelsResult> RebuildReadModelsAsync();
    Task SetRebuildScheduleAsync(string schedule);
    Task<string> GetRebuildScheduleAsync();
    Task<DateTime?> GetLastRebuildUtcAsync();
    Task<AuditLogQueryResult> GetJurisdictionAuditLogsAsync(AuditLogSearchRequest request);
    Task<AuditLogQueryResult> GetSuperAuditLogsAsync(AuditLogSearchRequest request);
}
