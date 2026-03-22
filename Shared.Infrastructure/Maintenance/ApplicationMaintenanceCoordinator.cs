using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modules.Records.Application.Admin.Commands.RebuildReadModels;
using Shared.Infrastructure.Locks;

namespace Shared.Infrastructure.Maintenance;

public sealed class ApplicationMaintenanceCoordinator
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly HashSet<Guid> _rebuiltJurisdictionsInCurrentWindow = [];
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ApplicationMaintenanceOptions _options;
    private readonly ILogger<ApplicationMaintenanceCoordinator> _logger;
    private DateTime _maintenanceWindowStartedUtc = DateTime.UtcNow;
    private DateTime _lastActivityUtc = DateTime.UtcNow;
    private bool _startupMaintenancePending = true;

    public ApplicationMaintenanceCoordinator(
        IOptions<ApplicationMaintenanceOptions> options,
        IHostApplicationLifetime hostApplicationLifetime,
        ILogger<ApplicationMaintenanceCoordinator> logger)
    {
        _options = options.Value;
        _hostApplicationLifetime = hostApplicationLifetime;
        _logger = logger;
    }

    public Task RunStartupMaintenanceAsync(
        IServiceProvider services,
        CancellationToken cancellationToken = default) =>
        RunAsync(services, Guid.Empty, true, cancellationToken);

    public Task RunRequestMaintenanceAsync(
        IServiceProvider services,
        Guid jurisdictionId,
        CancellationToken cancellationToken = default) =>
        RunAsync(services, jurisdictionId, false, cancellationToken);

    private async Task RunAsync(
        IServiceProvider services,
        Guid jurisdictionId,
        bool isStartup,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        // For request-triggered maintenance, skip (don't block) if a maintenance
        // run is already in progress so concurrent requests remain responsive.
        if (!isStartup && !_gate.Wait(0))
        {
            _lastActivityUtc = now;
            return;
        }

        if (isStartup)
            await _gate.WaitAsync(cancellationToken);

        try
        {
            var maintenanceCancellationToken = _hostApplicationLifetime.ApplicationStopping;

            if (isStartup)
            {
                if (_startupMaintenancePending)
                {
                    await TryRunGlobalMaintenanceAsync(services, maintenanceCancellationToken, "startup");
                    _maintenanceWindowStartedUtc = now;
                    _startupMaintenancePending = false;
                    _rebuiltJurisdictionsInCurrentWindow.Clear();
                    _logger.LogInformation(
                        "Startup maintenance window opened at {MaintenanceWindowStartedUtc}.",
                        _maintenanceWindowStartedUtc);
                }

                return;
            }

            var idleExceeded = now - _lastActivityUtc >= _options.InactivityThreshold;
            if (_startupMaintenancePending || idleExceeded)
            {
                var trigger = _startupMaintenancePending ? "startup-retry" : "idle-threshold";
                await TryRunGlobalMaintenanceAsync(services, maintenanceCancellationToken, trigger);
                _maintenanceWindowStartedUtc = now;
                _startupMaintenancePending = false;
                _rebuiltJurisdictionsInCurrentWindow.Clear();

                _logger.LogInformation(
                    "Opened a new maintenance window at {MaintenanceWindowStartedUtc}.",
                    _maintenanceWindowStartedUtc);
            }

            if (jurisdictionId == Guid.Empty)
                return;

            if (_rebuiltJurisdictionsInCurrentWindow.Contains(jurisdictionId))
                return;

            _logger.LogInformation(
                "Triggering automatic read-model rebuild for jurisdiction {JurisdictionId}.",
                jurisdictionId);

            await TryRunAutomaticRebuildAsync(services, jurisdictionId, maintenanceCancellationToken);
        }
        finally
        {
            _lastActivityUtc = now;
            _gate.Release();
        }
    }

    private async Task TryRunGlobalMaintenanceAsync(
        IServiceProvider services,
        CancellationToken cancellationToken,
        string trigger)
    {
        try
        {
            var lockCleanup = services.GetRequiredService<LockCleanupService>();
            await lockCleanup.ReleaseExpiredLocksAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Application maintenance lock cleanup failed during {Trigger}. Continuing without blocking startup or requests.",
                trigger);
        }
    }

    private async Task TryRunAutomaticRebuildAsync(
        IServiceProvider services,
        Guid jurisdictionId,
        CancellationToken cancellationToken)
    {
        try
        {
            var sender = services.GetRequiredService<ISender>();
            await sender.Send(new RebuildReadModelsCommand(), cancellationToken);
            _rebuiltJurisdictionsInCurrentWindow.Add(jurisdictionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Automatic read-model rebuild failed for jurisdiction {JurisdictionId}. Continuing the request pipeline.",
                jurisdictionId);
        }
    }
}
