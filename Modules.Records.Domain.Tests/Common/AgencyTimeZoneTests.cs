using Modules.Records.Domain.Common;

namespace Modules.Records.Domain.Tests.Common;

public class AgencyTimeZoneTests
{
    [Fact]
    public void FromConfigValue_ResolvesIanaId()
    {
        var zone = AgencyTimeZone.FromConfigValue("America/New_York");

        // 2026-01-15 12:00 UTC is 07:00 EST (UTC-5) — proves we got Eastern, not the Central default.
        var utc = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        Assert.Equal(7, TimeZoneInfo.ConvertTimeFromUtc(utc, zone).Hour);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Not/A_Zone")]
    public void FromConfigValue_FallsBackToCentral_WhenMissingOrUnrecognized(string? value)
    {
        var zone = AgencyTimeZone.FromConfigValue(value);

        // 2026-01-15 12:00 UTC is 06:00 CST (UTC-6) — the default America/Chicago zone.
        var utc = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        Assert.Equal(6, TimeZoneInfo.ConvertTimeFromUtc(utc, zone).Hour);
    }

    [Fact]
    public void FromConfigValue_AcceptsWindowsId()
    {
        // .NET maps Windows ids to IANA on Linux too, so an agency may store either form.
        var zone = AgencyTimeZone.FromConfigValue("Central Standard Time");

        var utc = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        Assert.Equal(6, TimeZoneInfo.ConvertTimeFromUtc(utc, zone).Hour);
    }
}
