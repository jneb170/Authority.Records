using Shared.Infrastructure.GoogleMaps;

namespace Infrastructure.IntegrationTests.Parsing;

/// <summary>
/// Unit tests for <see cref="AddressParser.ParseFormattedAddress"/>,
/// <see cref="AddressParser.NormalizeCountryCode"/>, and
/// <see cref="AddressParser.ParseStateZip"/>.
/// </summary>
public sealed class AddressParserTests
{
    // ── ParseFormattedAddress ─────────────────────────────────────────────────

    [Fact]
    public void ParseFormattedAddress_StandardUsAddress_ParsesAllFields()
    {
        var (streetNumber, route, aptSuite, city, state, zip, country) =
            AddressParser.ParseFormattedAddress("1370 S Rigsbee Dr, Plano, TX 75074, USA");

        Assert.Equal("1370",         streetNumber);
        Assert.Equal("S Rigsbee Dr", route);
        Assert.Null(aptSuite);
        Assert.Equal("Plano",        city);
        Assert.Equal("TX",           state);
        Assert.Equal("75074",        zip);
        Assert.Equal("US",           country);
    }

    [Fact]
    public void ParseFormattedAddress_SuiteInlineStreet_SplitsSuiteFromRoute()
    {
        var (streetNumber, route, aptSuite, city, state, zip, country) =
            AddressParser.ParseFormattedAddress("101 E Park Blvd Suite 600, Plano, TX 75074, USA");

        Assert.Equal("101",         streetNumber);
        Assert.Equal("E Park Blvd", route);
        Assert.Equal("Suite 600",   aptSuite);
        Assert.Equal("Plano",       city);
        Assert.Equal("TX",          state);
        Assert.Equal("75074",       zip);
        Assert.Equal("US",          country);
    }

    [Fact]
    public void ParseFormattedAddress_AptKeyword_SplitsAptFromRoute()
    {
        var (streetNumber, route, aptSuite, _, _, _, _) =
            AddressParser.ParseFormattedAddress("200 W Main St Apt 3B, Austin, TX 78701, USA");

        Assert.Equal("200",       streetNumber);
        Assert.Equal("W Main St", route);
        Assert.Equal("Apt 3B",    aptSuite);
    }

    [Fact]
    public void ParseFormattedAddress_NoStreetNumber_SetsRouteWithoutNumber()
    {
        var (streetNumber, route, aptSuite, city, state, zip, country) =
            AddressParser.ParseFormattedAddress("Main Street Clinic, Dallas, TX 75001, USA");

        Assert.Null(streetNumber);
        Assert.Equal("Main Street Clinic", route);
        Assert.Null(aptSuite);
        Assert.Equal("Dallas", city);
        Assert.Equal("TX",     state);
        Assert.Equal("75001",  zip);
        Assert.Equal("US",     country);
    }

    [Fact]
    public void ParseFormattedAddress_MissingCountry_ThreeParts_ParsesCityStateZip()
    {
        var (_, _, _, city, state, zip, country) =
            AddressParser.ParseFormattedAddress("123 Oak Ave, Chicago, IL 60601");

        Assert.Equal("Chicago", city);
        Assert.Equal("IL",      state);
        Assert.Equal("60601",   zip);
        Assert.Null(country);
    }

    [Fact]
    public void ParseFormattedAddress_TwoParts_ParsesCityOnly()
    {
        var (_, _, _, city, state, zip, country) =
            AddressParser.ParseFormattedAddress("Some Place, Houston");

        Assert.Equal("Houston", city);
        Assert.Null(state);
        Assert.Null(zip);
        Assert.Null(country);
    }

    [Fact]
    public void ParseFormattedAddress_SuiteOnlyNoStreet_SetsAptSuiteRouteNull()
    {
        var (streetNumber, route, aptSuite, city, _, _, _) =
            AddressParser.ParseFormattedAddress("Suite 600, Dallas, TX 75074, USA");

        Assert.Null(streetNumber);
        Assert.Null(route);
        Assert.Equal("Suite 600", aptSuite);
        Assert.Equal("Dallas",    city);
    }

    [Fact]
    public void ParseFormattedAddress_StateZipNoZip_ParsesStateOnly()
    {
        var (_, _, _, _, state, zip, _) =
            AddressParser.ParseFormattedAddress("500 Main St, Springfield, MO");

        Assert.Equal("MO", state);
        Assert.Null(zip);
    }

    [Fact]
    public void ParseFormattedAddress_FiveParts_CityIsThirdFromEnd()
    {
        // 5 parts: street | neighbourhood | city | state+zip | country
        var (_, _, _, city, state, zip, country) =
            AddressParser.ParseFormattedAddress(
                "100 Elm St, Uptown District, Nashville, TN 37201, USA");

        Assert.Equal("Nashville", city);
        Assert.Equal("TN",        state);
        Assert.Equal("37201",     zip);
        Assert.Equal("US",        country);
    }

    // ── NormalizeCountryCode ──────────────────────────────────────────────────

    [Theory]
    [InlineData("USA",                    "US")]
    [InlineData("usa",                    "US")]
    [InlineData("United States",          "US")]
    [InlineData("United States of America", "US")]
    [InlineData("Canada",                 "CA")]
    [InlineData("canada",                 "CA")]
    [InlineData("Mexico",                 "MX")]
    [InlineData("US",                     "US")]
    [InlineData("gb",                     "GB")]
    [InlineData(null,                     null)]
    [InlineData("",                       null)]
    [InlineData("   ",                    null)]
    public void NormalizeCountryCode_KnownValues_ReturnsExpected(
        string? input, string? expected)
    {
        Assert.Equal(expected, AddressParser.NormalizeCountryCode(input));
    }

    [Fact]
    public void NormalizeCountryCode_UnknownLongName_ReturnsNull()
    {
        Assert.Null(AddressParser.NormalizeCountryCode("Atlantis"));
    }

    // ── ParseStateZip ─────────────────────────────────────────────────────────

    [Fact]
    public void ParseStateZip_StateAndZip_BothParsed()
    {
        AddressParser.ParseStateZip("TX 75074", out var state, out var zip);
        Assert.Equal("TX",    state);
        Assert.Equal("75074", zip);
    }

    [Fact]
    public void ParseStateZip_StateOnly_ZipIsNull()
    {
        AddressParser.ParseStateZip("CA", out var state, out var zip);
        Assert.Equal("CA", state);
        Assert.Null(zip);
    }
}
