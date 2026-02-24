using Infrastructure.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Records.Domain.Abstractions;
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
            var tenantProvider = (TestTenantProvider)
                scope.ServiceProvider.GetRequiredService<ITenantProvider>();

            tenantProvider.SetJurisdictionId(tenantId);

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            aggregateId = Guid.NewGuid();
            db.Add(new FailingAggregate(aggregateId, tenantId));

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

            var message = await db.OutboxMessages
                .FirstAsync();

            Assert.Null(message.ProcessedOnUtc);
            Assert.True(message.IsFailedPermanently);
            Assert.Equal(1, message.RetryCount);
        }

        Assert.True(AlwaysFailingHandler.ExecutionCount >= 1);
    }
}
