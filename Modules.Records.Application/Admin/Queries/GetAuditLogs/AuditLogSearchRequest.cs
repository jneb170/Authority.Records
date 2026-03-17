namespace Modules.Records.Application.Admin.Queries.GetAuditLogs;

public sealed record AuditLogSearchRequest(
    string? Severity = null,
    string? RecordType = null,
    long? RecordNumber = null,
    string? ActionType = null,
    Guid? UserId = null,
    DateTime? FromUtc = null,
    DateTime? ToUtc = null,
    string? SearchText = null,
    string SortField = AuditLogSortFields.OccurredOnUtc,
    bool SortDescending = true,
    int PageNumber = 1,
    int PageSize = 50,
    string Scope = AuditLogScopes.All,
    Guid? JurisdictionId = null);
