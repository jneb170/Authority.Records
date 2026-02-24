using Infrastructure.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Outbox;
using Shared.Infrastructure.Persistence;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace Infrastructure.IntegrationTests.Outbox.Cleanup;

public sealed class TestOutboxCleanupTest : IntegrationTestBase
{
    [Fact]
    public async Task Cleanup_Should_Delete_Only_Old_Processed_Messages()
    {
        // Arrange
        var oldDate = DateTime.UtcNow.AddDays(-10);
        var recentDate = DateTime.UtcNow.AddDays(-1);

        using (var scope = ServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.OutboxMessages.AddRange(
                CreateProcessedMessage(oldDate, db),     // should delete
                CreateProcessedMessage(recentDate, db),  // should keep
                CreateUnprocessedMessage(db));         // should keep

            await db.SaveChangesAsync();
        }

        int deleted;

        // Act
        using (var scope = ServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var processor = scope.ServiceProvider.GetRequiredService<OutboxCleanupProcessor>();

            deleted = await processor.CleanupAsync(
                retentionPeriod: TimeSpan.FromDays(7));
        }

        // Assert
        using (var scope = ServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var remaining = await db.OutboxMessages.CountAsync();

            Assert.Equal(1, deleted);
            Assert.Equal(2, remaining);
        }
    }

    private static OutboxMessage CreateProcessedMessage(DateTime processedOnUtc, AppDbContext db)
    {
        OutboxMessage message = CreateUnprocessedMessage(db);
        message.MarkAsProcessed(processedOnUtc);
        
        return message;
    }

    private static OutboxMessage CreateUnprocessedMessage(AppDbContext db)
    {
        // Create valid message
        var message = new OutboxMessage(
            new StubDomainEvent(),
            Guid.NewGuid());

        return message;
    }

}
