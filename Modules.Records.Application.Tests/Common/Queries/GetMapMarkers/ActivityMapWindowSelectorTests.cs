using Modules.Records.Application.Common.Queries.GetMapMarkers;

namespace Modules.Records.Application.Tests.Common.Queries.GetMapMarkers;

public sealed class ActivityMapWindowSelectorTests
{
    [Fact]
    public void HasMinimumActivity_Returns_True_When_Two_Records_Are_Present()
    {
        var markers = new[]
        {
            CreateMarker(new DateTime(2026, 3, 17, 12, 0, 0, DateTimeKind.Utc)),
            CreateMarker(new DateTime(2026, 3, 17, 22, 0, 0, DateTimeKind.Utc))
        };

        var result = ActivityMapWindowSelector.HasMinimumActivity(markers);

        Assert.True(result);
    }

    [Fact]
    public void HasMinimumActivity_Returns_False_When_There_Is_Only_One_Record()
    {
        var markers = new[]
        {
            CreateMarker(new DateTime(2026, 3, 17, 12, 0, 0, DateTimeKind.Utc))
        };

        var result = ActivityMapWindowSelector.HasMinimumActivity(markers);

        Assert.False(result);
    }

    [Fact]
    public void HasMinimumActivity_Returns_False_For_Empty_Markers()
    {
        var result = ActivityMapWindowSelector.HasMinimumActivity(Array.Empty<MapMarkerDto>());

        Assert.False(result);
    }

    private static MapMarkerDto CreateMarker(DateTime occurredAt) =>
        new(
            "Incident",
            Guid.NewGuid(),
            1000,
            "INC-1000",
            "/incidents/1000",
            1,
            1,
            occurredAt);
}
