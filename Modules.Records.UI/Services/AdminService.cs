using MediatR;
using Modules.Records.Application.Admin.Commands.RebuildReadModels;
using Modules.Records.Application.Admin.Queries.GetAuditLogs;

namespace Modules.Records.UI.Services;

public sealed class AdminService : IAdminService
{
    private readonly ISender _sender;

    public AdminService(ISender sender) => _sender = sender;

    public Task<RebuildReadModelsResult> RebuildReadModelsAsync() =>
        _sender.Send(new RebuildReadModelsCommand());

    public Task<AuditLogQueryResult> GetJurisdictionAuditLogsAsync(AuditLogSearchRequest request) =>
        _sender.Send(new GetJurisdictionAuditLogsQuery(request));

    public Task<AuditLogQueryResult> GetSuperAuditLogsAsync(AuditLogSearchRequest request) =>
        _sender.Send(new GetSuperAuditLogsQuery(request));
}
