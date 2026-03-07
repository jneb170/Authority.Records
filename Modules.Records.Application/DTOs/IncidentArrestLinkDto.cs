namespace Modules.Records.Application.DTOs;

public sealed record IncidentArrestLinkDto(
    Guid LinkId,
    Guid IncidentId,
    long IncidentRecordNumber,
    string IncidentNum,
    Guid ArrestId,
    DateTime LinkedAtUtc);
