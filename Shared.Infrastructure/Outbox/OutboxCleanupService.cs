using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Persistence;

namespace Shared.Infrastructure.Outbox;

public sealed class OutboxCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxCleanupService> _logger;
    private readonly OutboxCleanupOptions _options;

    //private static readonly TimeSpan ExecutionInterval = TimeSpan.FromHours(6);
    //private static readonly TimeSpan RetentionPeriod = TimeSpan.FromDays(7);
    

    public OutboxCleanupService(
        IServiceScopeFactory scopeFactory,
        IOptions<OutboxCleanupOptions> options,
        ILogger<OutboxCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var processor = scope.ServiceProvider
                    .GetRequiredService<OutboxCleanupProcessor>();

                await processor.CleanupAsync(
                    _options.RetentionPeriod,
                    stoppingToken);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox cleanup failed.");
            }

            await Task.Delay(_options.ExecutionInterval, stoppingToken);
        }
    }
}
