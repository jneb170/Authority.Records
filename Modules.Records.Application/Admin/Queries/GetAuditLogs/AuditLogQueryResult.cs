namespace Modules.Records.Application.Admin.Queries.GetAuditLogs;

public sealed record AuditLogQueryResult(
    IReadOnlyList<AuditLogEntryDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    IReadOnlyList<string> AvailableSeverities,
    IReadOnlyList<string> AvailableRecordTypes,
    IReadOnlyList<string> AvailableActionTypes);
