using Infrastructure.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Outbox;
using Shared.Infrastructure.Persistence;
using Xunit;

namespace Infrastructure.IntegrationTests.Outbox.RetryBehavior;

public sealed class TestRetryBehavior : IntegrationTestBase
{
    [Fact]
    public async Task Outbox_Should_Mark_Message_As_Permanently_Failed_After_Max_Retries()
    {
        // Arrange
        AlwaysFailingHandler.Reset();

        var tenantId = Guid.NewGuid();

        Guid aggregateId;

        using (var scope = ServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            aggregateId = Guid.NewGuid();
            db.OutboxMessages.Add(new OutboxMessage(new FailingDomainEvent(aggregateId), tenantId));

            await db.SaveChangesAsync();
        }

        // Act
        using (var scope = ServiceProvider.CreateScope())
        {
            var processor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();

            await processor.ProcessOutboxMessages(CancellationToken.None);
        }

        // Assert
        using (var scope = ServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var outboxMessage = await db.OutboxMessages.FirstOrDefaultAsync();

            // Message should have been moved to the dead letter queue
            Assert.Null(outboxMessage);

            var deadLetterMessage = await db.DeadLetterMessages.FirstAsync();
            Assert.NotNull(deadLetterMessage.LastError);
            Assert.Equal(1, deadLetterMessage.RetryCount);
            Assert.Null(deadLetterMessage.RequeuedOnUtc);
        }

        Assert.Equal(1, AlwaysFailingHandler.ExecutionCount);
    }
}
