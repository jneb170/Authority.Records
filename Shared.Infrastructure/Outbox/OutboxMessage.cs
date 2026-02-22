using Modules.Records.Domain.DomainEvents;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Shared.Infrastructure.Outbox;

public sealed class OutboxMessage
{
    [Key]
    public Guid Id { get; private set; }

    public Guid JurisdictionId { get; private set; }

    [Required]
    public DateTime OccurredOnUtc { get; private set; }

    [Required]
    public string Type { get; private set; } = default!;

    [Required]
    public string Content { get; private set; } = default!;

    public DateTime? ProcessedOnUtc { get; private set; }

    public int RetryCount { get; private set; }

    public string? Error { get; private set; }

    public bool IsFailedPermanently { get; private set; }

    private OutboxMessage() { }

    public OutboxMessage(IDomainEvent domainEvent, Guid jurisdictionId)
    {
        if (domainEvent is null)
            throw new ArgumentNullException(nameof(domainEvent));

        if (!domainEvent.GetType().IsPublic)
            throw new InvalidOperationException(
                $"Domain event type {domainEvent.GetType().Name} must be public.");

        Id = Guid.NewGuid();
        JurisdictionId = jurisdictionId;
        OccurredOnUtc = DateTime.UtcNow;
        Type = domainEvent.GetType().AssemblyQualifiedName!;
        Content = JsonSerializer.Serialize(domainEvent,domainEvent.GetType());
    }

    public void MarkProcessed()
    {
        ProcessedOnUtc = DateTime.UtcNow;
        Error = null;
    }

    public void MarkFailed(string error, int maxRetries)
    {
        RetryCount++;
        Error = error;

        if (RetryCount >= maxRetries)
        {
            IsFailedPermanently = true;
        }
    }

    public bool CanBeProcessed() => ProcessedOnUtc == null && !IsFailedPermanently;
}
