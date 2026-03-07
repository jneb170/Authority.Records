namespace Shared.Infrastructure.Locks;

public sealed class LockCleanupOptions
{
    /// <summary>
    /// How long a lock can be held before it is considered expired.
    /// Must match the timeout used when acquiring locks (default: 10 minutes).
    /// </summary>
    public TimeSpan LockTimeout { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// How often the cleanup service checks for expired locks (default: 1 minute).
    /// </summary>
    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromMinutes(1);
}
