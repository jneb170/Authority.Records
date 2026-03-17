namespace Modules.Records.Application.ReadModels;

public sealed class AuditLogReadModel
{
    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Severity { get; private set; } = string.Empty;
    public DateTime OccurredOnUtc { get; private set; }
    public Guid? JurisdictionId { get; private set; }
    public Guid AggregateId { get; private set; }
    public long AggregateVersion { get; private set; }
    public string RecordType { get; private set; } = string.Empty;
    public string ActionType { get; private set; } = string.Empty;
    public Guid? UserId { get; private set; }
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
        string payload)
    {
        return new AuditLogReadModel
        {
            Id = id,
            EventId = eventId,
            EventType = eventType,
            Severity = severity,
            OccurredOnUtc = occurredOnUtc,
            JurisdictionId = jurisdictionId,
            AggregateId = aggregateId,
            AggregateVersion = aggregateVersion,
            RecordType = recordType,
            ActionType = actionType,
            UserId = userId,
            Message = message,
            Payload = payload
        };
    }
}
