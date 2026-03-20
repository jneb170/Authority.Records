namespace Modules.Records.Application.DTOs;

public sealed record CitationOfficerProfileDto(
    Guid? SourceNameId,
    long? SourceNameRecordNumber,
    string OfficerName,
    string? Title = null,
    string? BadgeOrIdentifier = null,
    string? UnitNumber = null);
