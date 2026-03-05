using System.ComponentModel.DataAnnotations;

namespace Shared.Infrastructure.Audit;

public sealed class AuditTrailEntry
{
    [Key]
    public Guid Id { get; private set; }

    public Guid EventId { get; private set; }

    [Required]
    public string EventType { get; private set; } = default!;

    public DateTime OccurredOnUtc { get; private set; }

    public Guid JurisdictionId { get; private set; }

    [Required]
    public string Payload { get; private set; } = default!;

    private AuditTrailEntry() { }

    public static AuditTrailEntry Create(
        Guid eventId,
        string eventType,
        DateTime occurredOnUtc,
        Guid jurisdictionId,
        string payload)
    {
        return new AuditTrailEntry
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            EventType = eventType,
            OccurredOnUtc = occurredOnUtc,
            JurisdictionId = jurisdictionId,
            Payload = payload
        };
    }
}
