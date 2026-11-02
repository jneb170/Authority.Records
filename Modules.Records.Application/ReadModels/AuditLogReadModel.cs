namespace Modules.Records.Application.ReadModels;

public sealed class AuditLogReadModel
{
    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Severity { get; private set; } = string.Empty;
    public DateTime OccurredOnUtc { get; private set; }
    public Guid? JurisdictionId { get; private set; }

    /// <summary>
    /// The agency whose audit trail this row belongs to: the owning agency of an incident,
    /// arrest or citation, otherwise the acting user's agency. Null for jurisdiction-wide rows
    /// — activity on the shared master index (names, locations) and system work — which every
    /// agency admin in the jurisdiction can see (issue #118).
    /// </summary>
    public Guid? AgencyId { get; private set; }
    public Guid AggregateId { get; private set; }
    public long AggregateVersion { get; private set; }
    public string RecordType { get; private set; } = string.Empty;
    public string ActionType { get; private set; } = string.Empty;
    public Guid? UserId { get; private set; }

    /// <summary>
    /// The actor's display name at the time the row was written, so the log still names the
    /// user after the account is renamed or deleted (issue #119). Null for system actions and
    /// for rows written before this column existed.
    /// </summary>
    public string? UserDisplayName { get; private set; }

    public string Message { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;

    private AuditLogReadModel() { }

    public static AuditLogReadModel Create(
        Guid id,
        Guid eventId,
        string eventType,
        string severity,
        DateTime occurredOnUtc,
        Guid? jurisdictionId,
        Guid aggregateId,
        long aggregateVersion,
        string recordType,
        string actionType,
        Guid? userId,
        string message,
        string payload,
        string? userDisplayName = null,
        Guid? agencyId = null)
    {
        return new AuditLogReadModel
        {
            Id = id,
            EventId = eventId,
            EventType = eventType,
            Severity = severity,
            OccurredOnUtc = occurredOnUtc,
            JurisdictionId = jurisdictionId,
            AgencyId = agencyId,
            AggregateId = aggregateId,
            AggregateVersion = aggregateVersion,
            RecordType = recordType,
            ActionType = actionType,
            UserId = userId,
            UserDisplayName = userDisplayName,
            Message = message,
            Payload = payload
        };
    }
}
