using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.DomainEvents;
using Polly;
using Polly.Retry;
using Shared.Infrastructure.Persistence;
using System.Text.Json;

namespace Shared.Infrastructure.Outbox;

public sealed class OutboxProcessor : BackgroundService
{
    private readonly int _maxRetries = 5;

    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly DomainEventTypeRegistry _typeRegistry;
    private readonly ILogger<OutboxProcessor> _logger;
    //private readonly AsyncRetryPolicy _retryPolicy;

    public OutboxProcessor(
        IServiceProvider serviceProvider,
        IServiceScopeFactory scopeFactory,
        DomainEventTypeRegistry typeRegistry,
        ILogger<OutboxProcessor> logger,
        int maxRetries = 5)
    {
        _serviceProvider = serviceProvider;
        _scopeFactory = scopeFactory;
        _typeRegistry = typeRegistry;
        _logger = logger;
        _maxRetries = maxRetries;

        // Configure retry policy with exponential backoff for transient failures
        //_retryPolicy = Policy
        //    .Handle<Exception>()
        //    .WaitAndRetryAsync(
        //        retryCount: _maxRetries,
        //        sleepDurationProvider: attempt =>
                    
        //            TimeSpan.FromMilliseconds(10), // small delay for tests
        //            //TODO: COMMENT ABOVE AND UNCOMMENT BELOW AFTER TESTING
        //            //TimeSpan.FromSeconds(Math.Pow(2, attempt)), //exponential
                    
        //        onRetry: (exception, timespan, retryCount, context) =>
        //        {
        //            _logger.LogWarning(
        //                exception,
        //                "Retry {RetryCount} after {Delay}s due to error.",
        //                retryCount,
        //                timespan.TotalSeconds);
        //        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessOutboxMessages(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
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
            }
            catch (Exception ex)
            {
                message.MarkFailed(ex.ToString(), _maxRetries);
            }

            if (message.IsFailedPermanently)
            {
                _logger.LogError(
                    "Outbox message {MessageId} ({Type}) permanently failed after {RetryCount} retries. Moving to dead letter queue.",
                    message.Id,
                    message.Type,
                    message.RetryCount);

                await deadLetterWriter.DeadLetterAsync(message, dbContext, cancellationToken);
                continue;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
