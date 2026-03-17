namespace Modules.Records.Application.Admin.Queries.GetAuditLogs;

public static class AuditLogSeverities
{
    public const string Information = "Information";
    public const string Warning = "Warning";
}

public static class AuditLogSortFields
{
    public const string OccurredOnUtc = "OccurredOnUtc";
    public const string Severity = "Severity";
    public const string RecordType = "RecordType";
    public const string ActionType = "ActionType";
    public const string EventType = "EventType";
}

public static class AuditLogScopes
{
    public const string All = "All";
    public const string Jurisdiction = "Jurisdiction";
    public const string System = "System";
}
