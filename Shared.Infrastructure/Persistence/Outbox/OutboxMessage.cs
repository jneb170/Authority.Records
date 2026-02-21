using Modules.Records.Domain.DomainEvents;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessage
{
    [Key]
    public Guid Id { get; private set; }

    [Required]
    public DateTime OccurredOnUtc { get; private set; }

    [Required]
    public string Type { get; private set; } = default!;

    [Required]
    public string Content { get; private set; } = default!;

    public DateTime? ProcessedOnUtc { get; private set; }

    public string? Error { get; private set; }

    private OutboxMessage() { }

    public OutboxMessage(IDomainEvent domainEvent)
    {
        if (domainEvent is null)
            throw new ArgumentNullException(nameof(domainEvent));

        if (!domainEvent.GetType().IsPublic)
            throw new InvalidOperationException(
                $"Domain event type {domainEvent.GetType().Name} must be public.");

        Id = Guid.NewGuid();
        OccurredOnUtc = DateTime.UtcNow;
        Type = domainEvent.GetType().FullName!;
        Content = System.Text.Json.JsonSerializer.Serialize(
            domainEvent,
            domainEvent.GetType());
    }

    public void MarkProcessed()
    {
        ProcessedOnUtc = DateTime.UtcNow;
        Error = null;
    }

    public void MarkFailed(string error)
    {
        Error = error;
    }
}
