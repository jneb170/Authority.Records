namespace Shared.Infrastructure.Maintenance;

public sealed class ApplicationMaintenanceOptions
{
    public TimeSpan InactivityThreshold { get; init; } = TimeSpan.FromHours(8);
}
