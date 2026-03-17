namespace Modules.Records.Application.Common.Queries.GetMapMarkers;

public static class ActivityMapWindowSelector
{
    public static readonly int[] CandidateWindowHours = [24, 168, 720, 2160, 0];

    public static bool HasMinimumActivity(
        IReadOnlyList<MapMarkerDto> markers,
        int minimumRecords = 2) =>
        markers.Count >= minimumRecords;
}
