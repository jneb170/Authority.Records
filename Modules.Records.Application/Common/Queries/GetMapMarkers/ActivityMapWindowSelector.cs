namespace Modules.Records.Application.Common.Queries.GetMapMarkers;

public static class ActivityMapWindowSelector
{
    public const int MinimumMarkers = 2;

    public static IReadOnlyList<int> CandidateWindowHours { get; } = [24, 168, 720, 2160, 0];

    public static bool HasMinimumActivity(
        IReadOnlyList<MapMarkerDto> markers,
        int minimumRecords = MinimumMarkers) =>
        markers.Count >= minimumRecords;
}
