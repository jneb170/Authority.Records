using Modules.Records.Domain.Common.Implementations;

namespace Modules.Records.Domain.Tests.Implementations;

public sealed class IncidentNumFormatterTests
{
    #region Format Tests

    [Fact]
    public void Format_WithYearToken_ReplacesYyyy()
    {
        var result = IncidentNumFormatter.Format("yyyy-001", 2026, 3, 7, 1);

        Assert.Equal("2026-001", result);
    }

    [Fact]
    public void Format_WithShortYearToken_ReplacesYy()
    {
        var result = IncidentNumFormatter.Format("yy-001", 2026, 3, 7, 1);

        Assert.Equal("26-001", result);
    }

    [Fact]
    public void Format_WithMonthToken_ReplacesMm()
    {
        var result = IncidentNumFormatter.Format("mm-001", 2026, 3, 7, 1);

        Assert.Equal("03-001", result);
    }

    [Fact]
    public void Format_WithDayToken_ReplacesDd()
    {
        var result = IncidentNumFormatter.Format("dd-001", 2026, 3, 7, 1);

        Assert.Equal("07-001", result);
    }

    [Fact]
    public void Format_WithSequenceToken_ZeroPadsToTokenLength()
    {
        var result = IncidentNumFormatter.Format("xxxxxx", 2026, 3, 7, 42);

        Assert.Equal("000042", result);
    }

    [Fact]
    public void Format_WithFullTemplate_FormatsCorrectly()
    {
        var result = IncidentNumFormatter.Format("yyyy-mmdd-xxxxxx", 2026, 3, 7, 42);

        Assert.Equal("2026-0307-000042", result);
    }

    [Fact]
    public void Format_WithShortSequenceToken_ZeroPadsToSingleDigitLength()
    {
        var result = IncidentNumFormatter.Format("xxx", 2026, 1, 1, 5);

        Assert.Equal("005", result);
    }

    [Fact]
    public void Format_WithLargeSequenceNumber_ExceedsTokenLength_StillFormats()
    {
        // sequence longer than token width is preserved
        var result = IncidentNumFormatter.Format("xxx", 2026, 1, 1, 12345);

        Assert.Equal("12345", result);
    }

    [Fact]
    public void Format_WithNoTokens_ReturnsTemplateUnchanged()
    {
        var result = IncidentNumFormatter.Format("STATICNUM", 2026, 1, 1, 1);

        Assert.Equal("STATICNUM", result);
    }

    [Fact]
    public void Format_WithDecemberAndDay31_PadsCorrectly()
    {
        var result = IncidentNumFormatter.Format("yyyy-mm-dd", 2026, 12, 31, 1);

        Assert.Equal("2026-12-31", result);
    }

    [Fact]
    public void Format_WithJanuaryAndDay1_PadsCorrectly()
    {
        var result = IncidentNumFormatter.Format("yyyy-mm-dd", 2026, 1, 1, 1);

        Assert.Equal("2026-01-01", result);
    }

    [Fact]
    public void Format_SequenceOne_WithSixXs_ProducesSixZerosPadded()
    {
        var result = IncidentNumFormatter.Format("xxxxxx", 2026, 1, 1, 1);

        Assert.Equal("000001", result);
    }

    #endregion

    #region Preview Tests

    [Fact]
    public void Preview_ReturnsNonEmptyString()
    {
        var result = IncidentNumFormatter.Preview("yyyy-xxxxxx");

        Assert.False(string.IsNullOrEmpty(result));
    }

    [Fact]
    public void Preview_UsesTodaysYear()
    {
        var today = DateTime.UtcNow;
        var expectedYear = today.Year.ToString("D4");

        var result = IncidentNumFormatter.Preview("yyyy-xxxxxx");

        Assert.StartsWith(expectedYear, result);
    }

    [Fact]
    public void Preview_UsesSequenceOne()
    {
        // Preview uses sequence 1, so six Xs becomes 000001
        var result = IncidentNumFormatter.Preview("xxxxxx");

        Assert.Equal("000001", result);
    }

    #endregion
}
