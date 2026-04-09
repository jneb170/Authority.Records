using Modules.Records.Application.Locations;

namespace Modules.Records.Application.Tests.Locations;

/// <summary>
/// Unit tests for <see cref="StreetParser.Parse"/>.
/// </summary>
public sealed class StreetParserTests
{
    // ── Test dictionaries ─────────────────────────────────────────────────────

    private static readonly Guid N  = Guid.NewGuid();
    private static readonly Guid S  = Guid.NewGuid();
    private static readonly Guid E  = Guid.NewGuid();
    private static readonly Guid W  = Guid.NewGuid();
    private static readonly Guid NW = Guid.NewGuid();
    private static readonly Guid NE = Guid.NewGuid();
    private static readonly Guid SE = Guid.NewGuid();
    private static readonly Guid SW = Guid.NewGuid();

    private static readonly Guid St   = Guid.NewGuid();
    private static readonly Guid Ave  = Guid.NewGuid();
    private static readonly Guid Blvd = Guid.NewGuid();
    private static readonly Guid Dr   = Guid.NewGuid();
    private static readonly Guid Rd   = Guid.NewGuid();
    private static readonly Guid Ct   = Guid.NewGuid();
    private static readonly Guid Ln   = Guid.NewGuid();
    private static readonly Guid Way  = Guid.NewGuid();

    private static readonly Dictionary<string, Guid> Directions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["N"] = N, ["S"] = S, ["E"] = E, ["W"] = W,
        ["NW"] = NW, ["NE"] = NE, ["SE"] = SE, ["SW"] = SW,
    };

    private static readonly Dictionary<string, Guid> StreetTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["St"] = St, ["Ave"] = Ave, ["Blvd"] = Blvd, ["Dr"] = Dr,
        ["Rd"] = Rd, ["Ct"] = Ct,  ["Ln"] = Ln,     ["Way"] = Way,
    };

    // ── Basic cases ───────────────────────────────────────────────────────────

    [Fact]
    public void Parse_PreDirectionAndStreetType_Resolved()
    {
        var (preDir, streetName, postDir, streetType) =
            StreetParser.Parse("S Rigsbee Dr", Directions, StreetTypes);

        Assert.Equal(S,          preDir);
        Assert.Equal("Rigsbee",  streetName);
        Assert.Null(postDir);
        Assert.Equal(Dr,         streetType);
    }

    [Fact]
    public void Parse_PostDirectionCheckedBeforeStreetType()
    {
        // "Oak Ave NW" — NW must be stripped as postDir BEFORE Ave is read as streetType.
        var (preDir, streetName, postDir, streetType) =
            StreetParser.Parse("Oak Ave NW", Directions, StreetTypes);

        Assert.Null(preDir);
        Assert.Equal("Oak", streetName);
        Assert.Equal(NW,    postDir);
        Assert.Equal(Ave,   streetType);
    }

    [Fact]
    public void Parse_AllFourComponents_Resolved()
    {
        // "N Oak Ave NW"
        var (preDir, streetName, postDir, streetType) =
            StreetParser.Parse("N Oak Ave NW", Directions, StreetTypes);

        Assert.Equal(N,     preDir);
        Assert.Equal("Oak", streetName);
        Assert.Equal(NW,    postDir);
        Assert.Equal(Ave,   streetType);
    }

    [Fact]
    public void Parse_NoDirectionsOrType_CoreNameOnly()
    {
        var (preDir, streetName, postDir, streetType) =
            StreetParser.Parse("Elm", Directions, StreetTypes);

        Assert.Null(preDir);
        Assert.Equal("Elm", streetName);
        Assert.Null(postDir);
        Assert.Null(streetType);
    }

    [Fact]
    public void Parse_StreetTypeOnly_NoDirections()
    {
        var (preDir, streetName, postDir, streetType) =
            StreetParser.Parse("Main St", Directions, StreetTypes);

        Assert.Null(preDir);
        Assert.Equal("Main", streetName);
        Assert.Null(postDir);
        Assert.Equal(St,     streetType);
    }

    // ── Synonyms ──────────────────────────────────────────────────────────────

    [Fact]
    public void Parse_FullNameStreetType_ResolvedViaSynonym()
    {
        // "E Park Boulevard" — "Boulevard" is a synonym for "Blvd"
        var (preDir, streetName, postDir, streetType) =
            StreetParser.Parse("E Park Boulevard", Directions, StreetTypes);

        Assert.Equal(E,      preDir);
        Assert.Equal("Park", streetName);
        Assert.Null(postDir);
        Assert.Equal(Blvd,   streetType);
    }

    [Fact]
    public void Parse_FullNameStreetType_Avenue_Synonym()
    {
        var (_, streetName, _, streetType) =
            StreetParser.Parse("Maple Avenue", Directions, StreetTypes);

        Assert.Equal("Maple", streetName);
        Assert.Equal(Ave,     streetType);
    }

    // ── Edge cases ────────────────────────────────────────────────────────────

    [Fact]
    public void Parse_NullRoute_ReturnsAllNulls()
    {
        var (preDir, streetName, postDir, streetType) =
            StreetParser.Parse(null, Directions, StreetTypes);

        Assert.Null(preDir);
        Assert.Null(streetName);
        Assert.Null(postDir);
        Assert.Null(streetType);
    }

    [Fact]
    public void Parse_EmptyRoute_ReturnsAllNulls()
    {
        var (preDir, streetName, postDir, streetType) =
            StreetParser.Parse("   ", Directions, StreetTypes);

        Assert.Null(preDir);
        Assert.Null(postDir);
        Assert.Null(streetType);
    }

    [Fact]
    public void Parse_SingleTokenDirection_DirectionConsumedStreetNameNull()
    {
        // Only "N" — pre-direction consumed, nothing left for street name.
        var (preDir, streetName, postDir, streetType) =
            StreetParser.Parse("N", Directions, StreetTypes);

        Assert.Equal(N,  preDir);
        Assert.Null(streetName);
        Assert.Null(postDir);
        Assert.Null(streetType);
    }

    [Fact]
    public void Parse_SingleTokenStreetType_StreetTypeConsumedNameNull()
    {
        // Only "St" — no direction, all consumed as street type, name is null.
        var (preDir, streetName, postDir, streetType) =
            StreetParser.Parse("St", Directions, StreetTypes);

        Assert.Null(preDir);
        Assert.Null(streetName);
        Assert.Null(postDir);
        Assert.Equal(St, streetType);
    }

    [Fact]
    public void Parse_TwoTokensDirectionSuffix_ConsumedAsPostDir()
    {
        // "Oak NW" — start=0, end=1, start < end is true, so "NW" is consumed as
        // post-direction. Result: preDir=null, streetName="Oak", postDir=NW.
        var (preDir, streetName, postDir, streetType) =
            StreetParser.Parse("Oak NW", Directions, StreetTypes);

        Assert.Null(preDir);
        Assert.Equal("Oak", streetName);
        Assert.Equal(NW,    postDir);
        Assert.Null(streetType);
    }

    [Fact]
    public void Parse_TwoTokensPreDirAndDirection_PostDirNotConsumedGuard()
    {
        // "N NW" — after pre-dir N is consumed, only "NW" remains (start==end).
        // post-direction check is start < end (strictly), so the guard fires and
        // "NW" is NOT consumed as postDir — it falls through to street-type check
        // (no match), leaving streetName="NW".
        var (preDir, streetName, postDir, streetType) =
            StreetParser.Parse("N NW", Directions, StreetTypes);

        Assert.Equal(N,    preDir);
        Assert.Equal("NW", streetName);
        Assert.Null(postDir);
        Assert.Null(streetType);
    }

    [Fact]
    public void Parse_MultiWordStreetName_JoinedCorrectly()
    {
        var (preDir, streetName, postDir, streetType) =
            StreetParser.Parse("N Martin Luther King Blvd", Directions, StreetTypes);

        Assert.Equal(N,                      preDir);
        Assert.Equal("Martin Luther King",   streetName);
        Assert.Null(postDir);
        Assert.Equal(Blvd,                   streetType);
    }

    [Fact]
    public void Parse_UnknownStreetType_IncludedInName()
    {
        // "Oak Crescent" — "Crescent" not in dict or synonyms → part of street name
        var (_, streetName, _, streetType) =
            StreetParser.Parse("Oak Crescent", Directions, StreetTypes);

        Assert.Equal("Oak Crescent", streetName);
        Assert.Null(streetType);
    }
}
