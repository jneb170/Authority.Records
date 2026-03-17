using MediatR;
using Modules.Records.Application.Admin.Commands.RebuildReadModels;
using Modules.Records.Application.Admin.Queries.GetAuditLogs;
using Modules.Records.Application.Configurations.Commands.SetAgencyConfiguration;
using Modules.Records.Application.Configurations.Queries.GetAgencyConfiguration;
using Modules.Records.Domain.Common;

namespace Modules.Records.UI.Services;

public sealed class AdminService : IAdminService
{
    private readonly ISender _sender;

    public AdminService(ISender sender) => _sender = sender;

    public Task<RebuildReadModelsResult> RebuildReadModelsAsync() =>
        _sender.Send(new RebuildReadModelsCommand());

    public async Task SetRebuildScheduleAsync(string schedule) =>
        await _sender.Send(new SetAgencyConfigurationCommand(
            ConfigurationKeys.ReadModelRebuildSchedule, schedule));

    public async Task<string> GetRebuildScheduleAsync()
    {
        var cfg = await _sender.Send(
            new GetAgencyConfigurationQuery(ConfigurationKeys.ReadModelRebuildSchedule));
        return cfg?.Value ?? "Off";
    }

    public async Task<DateTime?> GetLastRebuildUtcAsync()
    {
        var cfg = await _sender.Send(
            new GetAgencyConfigurationQuery(ConfigurationKeys.ReadModelRebuildLastRunUtc));
        if (cfg?.Value is null) return null;
        return DateTime.TryParse(cfg.Value, null,
            System.Globalization.DateTimeStyles.RoundtripKind, out var dt)
            ? dt
            : null;
    }

    public Task<AuditLogQueryResult> GetJurisdictionAuditLogsAsync(AuditLogSearchRequest request) =>
        _sender.Send(new GetJurisdictionAuditLogsQuery(request));

    public Task<AuditLogQueryResult> GetSuperAuditLogsAsync(AuditLogSearchRequest request) =>
        _sender.Send(new GetSuperAuditLogsQuery(request));
}
