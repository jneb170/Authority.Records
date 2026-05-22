namespace Modules.Records.Domain.Common;

/// <summary>
/// Converts a stored <see cref="ConfigurationKeys.LockTimeoutSeconds"/> value into a
/// <see cref="TimeSpan"/>, applying the system default when the value is missing,
/// non-numeric, or non-positive. Shared by the lock-acquisition handlers and the
/// background lock-cleanup sweep so both interpret the setting identically.
/// </summary>
public static class LockTimeout
{
    public static TimeSpan FromConfigValue(string? value) =>
        int.TryParse(value, out var seconds) && seconds > 0
            ? TimeSpan.FromSeconds(seconds)
            : TimeSpan.FromSeconds(ConfigurationKeys.DefaultLockTimeoutSeconds);
}
