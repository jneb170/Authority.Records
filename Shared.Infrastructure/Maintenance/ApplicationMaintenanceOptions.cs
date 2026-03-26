namespace Shared.Infrastructure.Maintenance;

public sealed class ApplicationMaintenanceOptions
{
    public TimeSpan InactivityThreshold { get; init; } = TimeSpan.FromMinutes(5);
    public TimeSpan OutboxPollingInterval { get; init; } = TimeSpan.FromSeconds(5);
}
