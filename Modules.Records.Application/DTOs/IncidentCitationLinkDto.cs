namespace Modules.Records.Application.DTOs;

public sealed record IncidentCitationLinkDto(
    Guid LinkId,
    Guid IncidentId,
    long IncidentRecordNumber,
    string IncidentNum,
    Guid CitationId,
    DateTime LinkedAtUtc);
