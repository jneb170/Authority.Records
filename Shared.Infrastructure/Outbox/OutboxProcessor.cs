using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.DomainEvents;
using Shared.Infrastructure.Maintenance;
using Shared.Infrastructure.Persistence;
using System.Text.Json;

namespace Shared.Infrastructure.Outbox;

public sealed class OutboxProcessor : BackgroundService
{
    private readonly int _maxRetries = 5;

    private readonly IServiceProvider _serviceProvider;
    private readonly DomainEventTypeRegistry _typeRegistry;
    private readonly IApplicationActivityTracker _activityTracker;
    private readonly ApplicationMaintenanceOptions _options;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(
        IServiceProvider serviceProvider,
        DomainEventTypeRegistry typeRegistry,
        IApplicationActivityTracker activityTracker,
        IOptions<ApplicationMaintenanceOptions> options,
        ILogger<OutboxProcessor> logger,
        int maxRetries = 5)
    {
        _serviceProvider = serviceProvider;
        _typeRegistry = typeRegistry;
        _activityTracker = activityTracker;
        _options = options.Value;
        _logger = logger;
        _maxRetries = maxRetries;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var observedActivityUtc = _activityTracker.LastActivityUtc;
                if (!_activityTracker.HasRecentActivity(_options.InactivityThreshold, DateTime.UtcNow))
                {
                    _logger.LogDebug(
                        "Outbox processor is idle. Waiting for authenticated activity before polling SQL again. LastActivityUtc: {LastActivityUtc}",
                        observedActivityUtc);

                    await _activityTracker.WaitForActivityAsync(observedActivityUtc, stoppingToken);
                    continue;
                }

                await ProcessOutboxMessages(stoppingToken);
                await Task.Delay(_options.OutboxPollingInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Graceful shutdown — leave the loop without logging an error.
                break;
            }
            catch (Exception ex)
            {
                // An unhandled exception here (e.g. the database being unreachable, as
                // happened during the SQLite cutover) must NOT propagate out of
                // ExecuteAsync: with the default BackgroundServiceExceptionBehavior.StopHost
                // that would crash the entire web host into a cold-start loop. Log, back
                // off, and let the loop retry on the next iteration.
                _logger.LogError(ex, "Outbox processor loop iteration failed. Backing off before retrying.");

                try
                {
                    await Task.Delay(_options.OutboxPollingInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

    public async Task ProcessOutboxMessages(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();
        var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantProvider>();
        var deadLetterWriter = scope.ServiceProvider.GetRequiredService<DeadLetterQueueWriter>();

        while (true)
        {
            //NOTE: It seems we could simplify the below to Where(m => m.CanBeProcessed()), but
            //      EF Core cannot translate arbitrary C# methods into SQL, so we need to
            //      include all fields in the query.
            var message = await dbContext.OutboxMessages
                .Where(m =>
                    m.ProcessedOnUtc == null &&
                    !m.IsFailedPermanently &&
                    m.ProcessingStartedOnUtc == null &&
                    (m.NextRetryOnUtc == null || m.NextRetryOnUtc <= DateTime.UtcNow))
                .OrderBy(m => m.OccurredOnUtc)
                .FirstOrDefaultAsync(cancellationToken);

            if (message is null)
                break;

            tenantProvider.SetJurisdictionId(message.JurisdictionId);

            //Distributed Idempotency via Optimistic Concurrency
            try
            {
                // Attempt atomic claim
                message.MarkProcessing();

                //await Task.Delay(TimeSpan.FromSeconds(4));

                await dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Started processing outbox message {MessageId} ({Type}) for jurisdiction {JurisdictionId}. RetryCount: {RetryCount}, OccurredOnUtc: {OccurredOnUtc}",
                    message.Id,
                    message.Type,
                    message.JurisdictionId,
                    message.RetryCount,
                    message.OccurredOnUtc);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Another processor claimed it
                dbContext.Entry(message).State = EntityState.Detached;
                continue;
            }

            try
            {
                //tenantProvider.SetJurisdictionId(message.JurisdictionId);

                if (!_typeRegistry.TryGet(message.Type, out var type) || type is null)
                    throw new InvalidOperationException($"Unknown type {message.Type}");

                var domainEvent = JsonSerializer.Deserialize(message.Content, type!)
                    as IDomainEvent
                    ?? throw new InvalidOperationException($"Invalid domain event payload: {message.Type}");

                await dispatcher.DispatchAsync(new[] { domainEvent }, cancellationToken);

                message.MarkProcessed();

                _logger.LogInformation(
                    "Processed outbox message {MessageId} ({Type}) for jurisdiction {JurisdictionId} successfully.",
                    message.Id,
                    message.Type,
                    message.JurisdictionId);
            }
            catch (Exception ex)
            {
                message.MarkFailed(ex.ToString(), _maxRetries);

                if (message.IsFailedPermanently)
                {
                    _logger.LogError(
                        ex,
                        "Outbox message {MessageId} ({Type}) permanently failed after {RetryCount} retries for jurisdiction {JurisdictionId}.",
                        message.Id,
                        message.Type,
                        message.RetryCount,
                        message.JurisdictionId);
                }
                else
                {
                    _logger.LogWarning(
                        ex,
                        "Outbox message {MessageId} ({Type}) failed attempt {RetryCount} for jurisdiction {JurisdictionId}. Next retry at {NextRetryOnUtc}.",
                        message.Id,
                        message.Type,
                        message.RetryCount,
                        message.JurisdictionId,
                        message.NextRetryOnUtc);
                }
            }

            if (message.IsFailedPermanently)
            {
                _logger.LogError(
                    "Outbox message {MessageId} ({Type}) is moving to the dead letter queue for jurisdiction {JurisdictionId}.",
                    message.Id,
                    message.Type,
                    message.JurisdictionId);

                await deadLetterWriter.DeadLetterAsync(message, dbContext, cancellationToken);
                continue;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
