using Modules.Records.Domain.Common;

namespace Shared.Infrastructure.Locks;

public sealed class LockCleanupOptions
{
    /// <summary>
    /// Fallback lock timeout for agencies that have no per-agency
    /// <see cref="ConfigurationKeys.LockTimeoutSeconds"/> setting. The per-agency
    /// value takes precedence; this default mirrors
    /// <see cref="ConfigurationKeys.DefaultLockTimeoutSeconds"/> (10 minutes) so the
    /// acquire-side and cleanup-side defaults stay in lock-step.
    /// </summary>
    public TimeSpan LockTimeout { get; set; } = TimeSpan.FromSeconds(ConfigurationKeys.DefaultLockTimeoutSeconds);

    /// <summary>
    /// How often the cleanup service checks for expired locks (default: 1 minute).
    /// </summary>
    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromMinutes(1);
}
