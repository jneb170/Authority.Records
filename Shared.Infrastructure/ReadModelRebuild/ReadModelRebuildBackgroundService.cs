using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Modules.Records.Application.Admin.Commands.RebuildReadModels;
using Modules.Records.Application.Configurations.Commands.SetAgencyConfiguration;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Shared.Infrastructure.Persistence;

namespace Shared.Infrastructure.ReadModelRebuild;

/// <summary>
/// Background service that checks every 15 minutes whether a scheduled read-model
/// rebuild is due for any jurisdiction that has configured one.
/// </summary>
public sealed class ReadModelRebuildBackgroundService : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(15);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReadModelRebuildBackgroundService> _logger;

    public ReadModelRebuildBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<ReadModelRebuildBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunDueRebuildsAsync(stoppingToken);
            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task RunDueRebuildsAsync(CancellationToken ct)
    {
        try
        {
            using var scope  = _scopeFactory.CreateScope();
            var db           = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantProvider>();
            var sender       = scope.ServiceProvider.GetRequiredService<ISender>();

            // Find all jurisdictions that have a rebuild schedule configured.
            var scheduleConfigs = await db.AgencyConfigurations
                .IgnoreQueryFilters()
                .Where(c => c.Key == ConfigurationKeys.ReadModelRebuildSchedule &&
                            c.Value != "Off")
                .ToListAsync(ct);

            foreach (var scheduleCfg in scheduleConfigs)
            {
                var jid = scheduleCfg.JurisdictionId;

                // Read last-run time for this jurisdiction.
                var lastRunCfg = await db.AgencyConfigurations
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c =>
                        c.JurisdictionId == jid &&
                        c.Key == ConfigurationKeys.ReadModelRebuildLastRunUtc, ct);

                DateTime? lastRun = null;
                if (lastRunCfg?.Value is not null &&
                    DateTime.TryParse(lastRunCfg.Value, null,
                        System.Globalization.DateTimeStyles.RoundtripKind, out var parsed))
                    lastRun = parsed;

                if (!IsRebuildDue(scheduleCfg.Value, lastRun)) continue;

                _logger.LogInformation(
                    "Scheduled read-model rebuild starting for jurisdiction {JurisdictionId} (schedule: {Schedule}).",
                    jid, scheduleCfg.Value);

                try
                {
                    tenantProvider.SetJurisdictionId(jid);
                    var result = await sender.Send(new RebuildReadModelsCommand(), ct);

                    // Persist the last-run timestamp.
                    var nowStr = DateTime.UtcNow.ToString("O");
                    await sender.Send(new SetAgencyConfigurationCommand(
                        ConfigurationKeys.ReadModelRebuildLastRunUtc, nowStr), ct);

                    _logger.LogInformation(
                        "Scheduled rebuild completed for {JurisdictionId}: " +
                        "{Names} names, {Arrests} arrests, {Citations} citations, " +
                        "{Incidents} incidents, {ArrestLinks} arrest links, {CitationLinks} citation links " +
                        "in {Elapsed:F2}s.",
                        jid,
                        result.NamesRebuilt, result.ArrestsRebuilt, result.CitationsRebuilt,
                        result.IncidentsRebuilt, result.ArrestLinksRebuilt, result.CitationLinksRebuilt,
                        result.Elapsed.TotalSeconds);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Scheduled read-model rebuild failed for jurisdiction {JurisdictionId}.", jid);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ReadModelRebuildBackgroundService encountered an error.");
        }
    }

    private static bool IsRebuildDue(string schedule, DateTime? lastRun)
    {
        var interval = schedule switch
        {
            "Hourly"    => TimeSpan.FromHours(1),
            "TwiceDaily" => TimeSpan.FromHours(12),
            "Daily"     => TimeSpan.FromHours(24),
            _           => (TimeSpan?)null
        };

        if (interval is null) return false;
        if (lastRun is null)  return true;

        return DateTime.UtcNow - lastRun.Value >= interval;
    }
}
