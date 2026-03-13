namespace Modules.Records.Application.Common.Queries.GetMapMarkers;

/// <summary>
/// A single map pin representing an Incident, Arrest, or Citation that has a parseable GPS coordinate.
/// </summary>
public sealed record MapMarkerDto(
    /// <summary>"Incident", "Arrest", or "Citation"</summary>
    string    RecordType,
    Guid      RecordId,
    long      RecordNumber,
    /// <summary>Display label shown in the map info-window (e.g. "INC-001" or "#10005")</summary>
    string    Label,
    /// <summary>Navigation URL for clicking the marker</summary>
    string    Url,
    double    Lat,
    double    Lng,
    DateTime  OccurredAt);
