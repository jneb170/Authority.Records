using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.DomainEvents;
using Shared.Infrastructure.Persistence;
using Shared.Infrastructure.Persistence.Outbox;
using System.Text.Json;

namespace Shared.Infrastructure.Outbox;

public sealed class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public OutboxProcessor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessOutboxMessages(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task ProcessOutboxMessages(CancellationToken cancellationToken)
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
                if (!DomainEventTypeRegistry.TryGet(message.Type, out var type))
                {
                    message.MarkFailed($"Unknown domain event type: {message.Type}");
                    continue;
                }

                var domainEvent = JsonSerializer.Deserialize(message.Content, type);

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
