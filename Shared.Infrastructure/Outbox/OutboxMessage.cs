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

    public DateTime? ProcessingStartedOnUtc { get; private set; }

    public int RetryCount { get; private set; }

    public string? Error { get; private set; }

    public bool IsFailedPermanently { get; private set; }

    // Concurrency token for optimistic locking
    //  Requires a default value or will generate exception
    //  Microsoft.Data.Sqlite.SqliteException : SQLite Error 19: 'NOT NULL constraint failed: OutboxMessages.RowVersion'
    //  when testing.
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public DateTime? NextRetryOnUtc { get; private set; }


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

    public void MarkProcessing()
    {
        ProcessingStartedOnUtc = DateTime.UtcNow;
    }

    public void MarkProcessed()
    {
        ProcessedOnUtc = DateTime.UtcNow;
        ProcessingStartedOnUtc = null;
        Error = null;
    }

    public void MarkFailed(string error, int maxRetries)
    {
        RetryCount++;
        Error = error;
        ProcessingStartedOnUtc = null;

        if (RetryCount >= maxRetries)
        {
            IsFailedPermanently = true;
            NextRetryOnUtc = null;
            return;
        }

        // Exponential backoff (2^retryCount seconds)
        var delaySeconds = Math.Pow(2, RetryCount);

        NextRetryOnUtc = DateTime.UtcNow.AddSeconds(delaySeconds);
    }

    public bool CanBeProcessed() => 
        ProcessedOnUtc == null 
        && !IsFailedPermanently
        && ProcessingStartedOnUtc == null
        && (NextRetryOnUtc == null || NextRetryOnUtc <= DateTime.UtcNow);
}
