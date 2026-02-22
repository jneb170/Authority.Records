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

    private readonly AsyncRetryPolicy _retryPolicy;

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
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: _maxRetries,
                sleepDurationProvider: attempt =>
                    
                    TimeSpan.FromMilliseconds(10), // small delay for tests
                    //TODO: COMMENT ABOVE AND UNCOMMENT BELOW AFTER TESTING
                    //TimeSpan.FromSeconds(Math.Pow(2, attempt)), //exponential
                    
                onRetry: (exception, timespan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        exception,
                        "Retry {RetryCount} after {Delay}s due to error.",
                        retryCount,
                        timespan.TotalSeconds);
                });
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

        var messages = await dbContext.OutboxMessages
            .Where(x => x.ProcessedOnUtc == null)
            .OrderBy(x => x.OccurredOnUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    // Restore tenant context
                    tenantProvider.SetJurisdictionId(message.JurisdictionId);
                    
                    if (!_typeRegistry.TryGet(message.Type, out var type))
                    {
                        throw new InvalidOperationException(
                            $"Unknown domain event type: {message.Type}");
                    }

                    var domainEvent = JsonSerializer.Deserialize(message.Content, type!);

                    // "is not" pattern matching below performs 2 operation simultaneously:
                    //  1.	Type Check: Tests if domainEvent (which is of type object? from JsonSerializer.Deserialize)
                    //      implements the IDomainEvent interface
                    //  2.  Cast + Assignment: If the type check succeeds, casts domainEvent to IDomainEvent and assigns
                    //      it to a new variable called typedEvent
                    if (domainEvent is not IDomainEvent typedEvent)
                    {
                        throw new InvalidOperationException(
                            $"Invalid domain event payload: {message.Type}");
                    }

                    await dispatcher.DispatchAsync(typedEvent, cancellationToken);
                });

                message.MarkProcessed();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Outbox message {MessageId} failed after retries.",
                    message.Id);

                message.MarkFailed(ex.ToString(), _maxRetries);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
