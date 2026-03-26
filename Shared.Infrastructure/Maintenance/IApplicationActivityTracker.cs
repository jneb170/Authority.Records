namespace Shared.Infrastructure.Maintenance;

public interface IApplicationActivityTracker
{
    DateTime LastActivityUtc { get; }
    bool HasRecentActivity(TimeSpan threshold, DateTime nowUtc);
    void RecordActivity(DateTime? activityUtc = null);
    Task WaitForActivityAsync(DateTime observedLastActivityUtc, CancellationToken cancellationToken);
}
