using System.Globalization;
using Modules.Records.Domain.Common;

namespace Modules.Records.UI.Services;

/// <summary>
/// Converts stored UTC timestamps to the active agency's wall-clock time (issue #95).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="DateTime.ToLocalTime"/> in Blazor Server converts to the <em>server's</em> zone, which is UTC in
/// the production container, so a Texas agency saw every time 5-6 hours ahead (and <see cref="DateTime.Today"/>
/// rolled over at 6-7 PM). The agency's <see cref="ConfigurationKeys.TimeZoneId"/> (default Central) is resolved
/// once per circuit, the same way the Texas citation PDF does it.
/// </para>
/// <para>
/// <see cref="Shared.MainLayout"/> awaits <see cref="EnsureLoadedAsync"/> before rendering the page body, so
/// pages and components can call the synchronous members. Pages rendered without MainLayout (print layouts)
/// must await it themselves. Until loaded, the default zone is used.
/// </para>
/// </remarks>
public interface IAgencyClock
{
    TimeZoneInfo TimeZone { get; }

    /// <summary>Loads the active agency's zone (no-op once loaded). Never throws: falls back to the default zone.</summary>
    Task EnsureLoadedAsync();

    /// <summary>Forgets the loaded zone, e.g. after the active agency changes.</summary>
    void Reset();

    /// <summary>Current wall-clock time in the agency's zone.</summary>
    DateTime Now { get; }

    /// <summary>Today's date in the agency's zone.</summary>
    DateTime Today { get; }

    /// <summary>Converts a stored UTC timestamp to agency wall-clock time.</summary>
    DateTime ToAgencyTime(DateTime utc);

    /// <summary>Converts an agency wall-clock time (e.g. from a datetime-local input) to UTC.</summary>
    DateTime ToUtc(DateTime agencyLocal);

    /// <summary>Formats a stored UTC timestamp in agency time, optionally followed by the zone abbreviation (CST/CDT).</summary>
    string Format(DateTime utc, string format, bool withZone = false);

    /// <summary>Formats a nullable UTC timestamp; returns <paramref name="empty"/> when null.</summary>
    string Format(DateTime? utc, string format, bool withZone = false, string empty = "");
}

public sealed class AgencyClock(IAgencyConfigurationService configService, ILogger<AgencyClock> logger, TimeProvider timeProvider) : IAgencyClock
{
    private Task? _loading;

    public TimeZoneInfo TimeZone { get; private set; } = AgencyTimeZone.FromConfigValue(null);

    public Task EnsureLoadedAsync() => _loading ??= LoadAsync();

    public void Reset() => _loading = null;

    private async Task LoadAsync()
    {
        try
        {
            var setting = await configService.GetAsync(ConfigurationKeys.TimeZoneId);
            TimeZone = AgencyTimeZone.FromConfigValue(setting?.Value);
        }
        catch (Exception ex)
        {
            // Super users have no agency, and a failed lookup must never break rendering.
            logger.LogDebug(ex, "Could not load the agency time zone; using the default.");
            TimeZone = AgencyTimeZone.FromConfigValue(null);
        }
    }

    public DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(timeProvider.GetUtcNow().UtcDateTime, TimeZone);

    public DateTime Today => Now.Date;

    public DateTime ToAgencyTime(DateTime utc) => ToAgencyTime(utc, TimeZone);

    public DateTime ToUtc(DateTime agencyLocal)
    {
        var unspecified = DateTime.SpecifyKind(agencyLocal, DateTimeKind.Unspecified);
        // A wall-clock time skipped by a DST jump has no UTC equivalent; nudge it forward an hour.
        if (TimeZone.IsInvalidTime(unspecified))
            unspecified = unspecified.AddHours(1);
        return TimeZoneInfo.ConvertTimeToUtc(unspecified, TimeZone);
    }

    public string Format(DateTime utc, string format, bool withZone = false) => Format(utc, format, TimeZone, withZone);

    public string Format(DateTime? utc, string format, bool withZone = false, string empty = "")
        => utc.HasValue ? Format(utc.Value, format, withZone) : empty;

    internal static DateTime ToAgencyTime(DateTime utc, TimeZoneInfo zone)
        // EF materializes SQLite DateTimes as Kind=Unspecified; they are stored as UTC. ConvertTimeFromUtc throws
        // on Kind=Local, so normalise first (a Local value is converted to UTC properly).
        => TimeZoneInfo.ConvertTimeFromUtc(
            utc.Kind == DateTimeKind.Local ? utc.ToUniversalTime() : DateTime.SpecifyKind(utc, DateTimeKind.Utc),
            zone);

    internal static string Format(DateTime utc, string format, TimeZoneInfo zone, bool withZone)
    {
        var local = ToAgencyTime(utc, zone);
        var text = local.ToString(format, CultureInfo.CurrentCulture);
        return withZone ? $"{text} {Abbreviation(local, zone)}" : text;
    }

    /// <summary>
    /// "CST"/"CDT" style abbreviation from the zone's standard/daylight name ("Central Daylight Time"), or a UTC
    /// offset ("UTC-05:00") when the name doesn't abbreviate cleanly.
    /// </summary>
    internal static string Abbreviation(DateTime agencyLocal, TimeZoneInfo zone)
    {
        if (zone == TimeZoneInfo.Utc)
            return "UTC";

        var name = zone.IsDaylightSavingTime(agencyLocal) ? zone.DaylightName : zone.StandardName;
        var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length is >= 2 and <= 4 && words.All(w => char.IsLetter(w[0])))
            return string.Concat(words.Select(w => char.ToUpperInvariant(w[0])));

        var offset = zone.GetUtcOffset(agencyLocal);
        return $"UTC{(offset < TimeSpan.Zero ? "-" : "+")}{offset.Duration():hh\\:mm}";
    }
}
