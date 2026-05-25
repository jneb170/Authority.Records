namespace Modules.Records.Domain.Common;

/// <summary>
/// Resolves a stored <see cref="ConfigurationKeys.TimeZoneId"/> value into a
/// <see cref="TimeZoneInfo"/>, applying the <see cref="ConfigurationKeys.DefaultTimeZoneId"/>
/// (Central) when the value is missing or unrecognized, and falling back to UTC only if even the
/// default cannot be resolved on the host. Mirrors <see cref="LockTimeout.FromConfigValue"/> so the
/// timezone setting is interpreted identically wherever it is read.
/// </summary>
public static class AgencyTimeZone
{
    public static TimeZoneInfo FromConfigValue(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) && TryResolve(value.Trim(), out var configured))
            return configured;

        if (TryResolve(ConfigurationKeys.DefaultTimeZoneId, out var fallback))
            return fallback;

        // A stripped host that can't even resolve the default still must not crash the print.
        return TimeZoneInfo.Utc;
    }

    private static bool TryResolve(string id, out TimeZoneInfo zone)
    {
        try
        {
            // .NET resolves both IANA ("America/Chicago") and Windows ("Central Standard Time")
            // ids on Windows and Linux, so an agency may store whichever form it knows.
            zone = TimeZoneInfo.FindSystemTimeZoneById(id);
            return true;
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            zone = TimeZoneInfo.Utc;
            return false;
        }
    }
}
