namespace Modules.Records.Application.Common.Queries.GetMapMarkers;

public sealed record ActivityMapWindowState(
    int WindowHours,
    IReadOnlyList<MapMarkerDto> Markers);
