using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Persistence;

namespace Shared.Infrastructure.Outbox;

public sealed class OutboxCleanupProcessor
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<OutboxCleanupProcessor> _logger;

    public OutboxCleanupProcessor(
        AppDbContext dbContext,
        ILogger<OutboxCleanupProcessor> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<int> CleanupAsync(
        TimeSpan retentionPeriod,
        CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.Subtract(retentionPeriod);

        var deleted = await _dbContext.OutboxMessages
            .Where(m =>
                m.ProcessedOnUtc != null &&
                m.ProcessedOnUtc < cutoffDate)
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted > 0)
        {
            _logger.LogInformation(
                "Outbox cleanup removed {Count} messages older than {Cutoff}.",
                deleted,
                cutoffDate);
        }

        var deadLetterDeleted = await _dbContext.DeadLetterMessages
            .Where(m =>
                m.RequeuedOnUtc != null &&
                m.RequeuedOnUtc < cutoffDate)
            .ExecuteDeleteAsync(cancellationToken);

        if (deadLetterDeleted > 0)
        {
            _logger.LogInformation(
                "Outbox cleanup removed {Count} dead letter messages older than {Cutoff}.",
                deadLetterDeleted,
                cutoffDate);
        }

        return deleted + deadLetterDeleted;
    }
}
