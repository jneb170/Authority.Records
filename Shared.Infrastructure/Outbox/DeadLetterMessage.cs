using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Infrastructure.Outbox;

public sealed class DeadLetterMessage
{
    [Key]
    public Guid Id { get; private set; }

    public Guid OriginalMessageId { get; private set; }

    public Guid JurisdictionId { get; private set; }

    [Required]
    public DateTime OccurredOnUtc { get; private set; }

    [Required]
    public string Type { get; private set; } = default!;

    [Required]
    public string Content { get; private set; } = default!;

    public DateTime DeadLetteredOnUtc { get; private set; }

    public string? LastError { get; private set; }

    public int RetryCount { get; private set; }

    public DateTime? RequeuedOnUtc { get; private set; }

    private DeadLetterMessage() { }

    public static DeadLetterMessage From(OutboxMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        return new DeadLetterMessage
        {
            Id = Guid.NewGuid(),
            OriginalMessageId = message.Id,
            JurisdictionId = message.JurisdictionId,
            OccurredOnUtc = message.OccurredOnUtc,
            Type = message.Type,
            Content = message.Content,
            DeadLetteredOnUtc = DateTime.UtcNow,
            LastError = message.Error,
            RetryCount = message.RetryCount
        };
    }

    public void MarkRequeued()
    {
        if (RequeuedOnUtc.HasValue)
            throw new InvalidOperationException("Message has already been requeued.");

        RequeuedOnUtc = DateTime.UtcNow;
    }

    public bool IsRequeued => RequeuedOnUtc.HasValue;
}
