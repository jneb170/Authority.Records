using System.Globalization;
using Microsoft.Extensions.Logging.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.UI.Helpers;
using Modules.Records.UI.Services;
using Modules.Records.UI.Tests.Printing;

namespace Modules.Records.UI.Tests.Time;

/// <summary>
/// Issue #95: the UI used DateTime.ToLocalTime(), i.e. the server's zone (UTC in production). The agency clock
/// never consults the server zone, so these hold on a UTC host (CI, production) as well as on a developer laptop.
/// </summary>
public sealed class AgencyClockTests : IDisposable
{
    // 9:06 PM Central on Sunday 1 Nov 2026 (DST ended that morning, so CST, UTC-6).
    private static readonly DateTime EveningUtc = new(2026, 11, 2, 3, 6, 0, DateTimeKind.Utc);
    private readonly CultureInfo _previousCulture = CultureInfo.CurrentCulture;

    public AgencyClockTests() => CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

    public void Dispose() => CultureInfo.CurrentCulture = _previousCulture;

    [Fact]
    public async Task DefaultsToCentral_WhenAgencyHasNoTimeZone()
    {
        var clock = await CreateClockAsync(timeZoneId: null);

        Assert.Equal("11/01/2026 9:06 PM CST", clock.Format(EveningUtc, "MM/dd/yyyy h:mm tt", withZone: true));
    }

    [Fact]
    public async Task UsesTheAgencyConfiguredZone()
    {
        var clock = await CreateClockAsync("America/New_York");

        Assert.Equal("11/01/2026 10:06 PM EST", clock.Format(EveningUtc, "MM/dd/yyyy h:mm tt", withZone: true));
    }

    [Fact]
    public async Task ShowsDaylightAbbreviation_InSummer()
    {
        var clock = await CreateClockAsync("America/Chicago");

        Assert.Equal("7/1/2026 10:00 AM CDT", clock.Format(new DateTime(2026, 7, 1, 15, 0, 0, DateTimeKind.Utc), "M/d/yyyy h:mm tt", withZone: true));
    }

    [Fact]
    public async Task FallsBackToCentral_ForAnUnknownZone()
    {
        var clock = await CreateClockAsync("Not/AZone");

        Assert.Equal("9:06 PM", clock.Format(EveningUtc, "h:mm tt"));
    }

    [Fact]
    public async Task TreatsUnspecifiedKindAsUtc_AsSqliteMaterializesIt()
    {
        var clock = await CreateClockAsync(null);
        var unspecified = DateTime.SpecifyKind(EveningUtc, DateTimeKind.Unspecified);

        Assert.Equal(new DateTime(2026, 11, 1, 21, 6, 0), clock.ToAgencyTime(unspecified));
    }

    [Fact]
    public async Task Today_RollsOverAtAgencyMidnight_NotUtcMidnight()
    {
        var clock = await CreateClockAsync(null, now: EveningUtc);

        Assert.Equal(new DateTime(2026, 11, 1), clock.Today);
    }

    [Fact]
    public async Task DateTimeLocalInputs_RoundTripThroughAgencyTime()
    {
        var clock = await CreateClockAsync(null);

        var input = DateTimeLocalInputHelper.FormatLocal(EveningUtc, clock);
        Assert.Equal("2026-11-01T21:06", input);

        Assert.True(DateTimeLocalInputHelper.TryParseUtcFromLocal(input, clock, out var parsed));
        Assert.Equal(EveningUtc, parsed);
        Assert.Equal(DateTimeKind.Utc, parsed!.Value.Kind);
    }

    [Fact]
    public async Task FormatsNullAsTheEmptyValue()
    {
        var clock = await CreateClockAsync(null);

        Assert.Equal("—", clock.Format((DateTime?)null, "g", empty: "—"));
    }

    private static async Task<AgencyClock> CreateClockAsync(string? timeZoneId, DateTime? now = null)
    {
        var values = timeZoneId is null ? new Dictionary<string, string>() : new() { [ConfigurationKeys.TimeZoneId] = timeZoneId };
        var clock = new AgencyClock(
            new FakeAgencyConfigurationService(values),
            NullLogger<AgencyClock>.Instance,
            new FixedTimeProvider(now ?? EveningUtc));
        await clock.EnsureLoadedAsync();
        return clock;
    }

    private sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow, TimeSpan.Zero);
    }
}
