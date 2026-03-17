namespace Modules.Records.Application.Admin.Queries.GetAuditLogs;

public sealed record AuditLogEntryDto(
    Guid Id,
    DateTime OccurredOnUtc,
    string Severity,
    string RecordType,
    long? RecordNumber,
    string? NavigationUrl,
    string ActionType,
    string EventType,
    Guid? JurisdictionId,
    Guid AggregateId,
    long AggregateVersion,
    Guid? UserId,
    string ActorDisplayName,
    string Message,
    string Payload);
