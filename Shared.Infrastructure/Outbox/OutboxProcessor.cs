using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.DomainEvents;
using Shared.Infrastructure.Persistence;
using System.Text.Json;

namespace Shared.Infrastructure.Outbox;

public sealed class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly DomainEventTypeRegistry _typeRegistry;

    public OutboxProcessor(
        IServiceProvider serviceProvider,
        IServiceScopeFactory scopeFactory,
        DomainEventTypeRegistry typeRegistry)
    {
        _serviceProvider = serviceProvider;
        _scopeFactory = scopeFactory;
        _typeRegistry = typeRegistry;
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

        var messages = await dbContext.OutboxMessages
            .Where(x => x.ProcessedOnUtc == null)
            .OrderBy(x => x.OccurredOnUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantProvider>();

                //set correct tenant context before dispatching message
                tenantProvider.SetJurisdictionId(message.JurisdictionId);

                if (!_typeRegistry.TryGet(message.Type, out var type))
                {
                    message.MarkFailed($"Unknown domain event type: {message.Type}");
                    continue;
                }

                var domainEvent = JsonSerializer.Deserialize(message.Content, type!);

                if (domainEvent is not IDomainEvent typedEvent)
                {
                    message.MarkFailed($"Deserialized type does not implement IDomainEvent: {message.Type}");
                    continue;
                }

                await dispatcher.DispatchAsync(typedEvent, cancellationToken);
                message.MarkProcessed();
            }
            catch (Exception ex)
            {
                message.MarkFailed(ex.ToString());
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
