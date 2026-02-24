using Infrastructure.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Records.Domain.Abstractions;
using Shared.Infrastructure.Outbox;
using Shared.Infrastructure.Persistence;
using Infrastructure.IntegrationTests.Outbox.Idempotency;

namespace Infrastructure.IntegrationTests.Outbox.Idempotency
{
    
    public sealed class TestIdempotency : IntegrationTestBase
    {
        [Fact]
        public async Task Outbox_Should_Process_Message_Only_Once_When_Processors_Run_Concurrently()
        {
            // Arrange
            CountingHandler.Reset();
            var tenantId = Guid.NewGuid();

            // Create message
            using (var scope = ServiceProvider.CreateScope())
            {
                var tenantProvider = (TestTenantProvider)
                    scope.ServiceProvider.GetRequiredService<ITenantProvider>();

                tenantProvider.SetJurisdictionId(tenantId);

                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                db.Add(new TestIdempotencyAggregate(Guid.NewGuid(), tenantId));
                await db.SaveChangesAsync();
            }

            // Act — simulate two processors competing
            var task1 = Task.Run(async () =>
            {
                using var scope = ServiceProvider.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();
                await processor.ProcessOutboxMessages(CancellationToken.None);
            });

            var task2 = Task.Run(async () =>
            {
                using var scope = ServiceProvider.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();
                await processor.ProcessOutboxMessages(CancellationToken.None);
            });

            await Task.WhenAll(task1, task2);

            // Assert
            using (var scope = ServiceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var message = await db.OutboxMessages.FirstAsync();

                Assert.NotNull(message.ProcessedOnUtc);
                Assert.False(message.IsFailedPermanently);
            }

            Assert.Equal(1, CountingHandler.ExecutionCount);
        }

    }
}
