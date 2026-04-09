namespace Modules.Records.Application.Relationships.Queries.GetRecordRelationships;

public sealed record RecordRelationshipsDto(
    RecordRelationshipSourceDto Source,
    IReadOnlyList<RecordRelationshipGroupDto> Groups);

public sealed record RecordRelationshipSourceDto(
    string RecordType,
    long RecordNumber,
    string Title,
    string? Subtitle,
    string NavigationUrl);

public sealed record RecordRelationshipGroupDto(
    string Title,
    IReadOnlyList<RecordRelationshipItemDto> Items);

public sealed record RecordRelationshipItemDto(
    string RecordType,
    long RecordNumber,
    string Title,
    string? Subtitle,
    string NavigationUrl,
    string? RelationshipLabel = null);
