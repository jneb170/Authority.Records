using Shared.Infrastructure.Outbox;
using Infrastructure.IntegrationTests.Common;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Persistence;

namespace Infrastructure.IntegrationTests.Outbox.TenantIsolation;

public class OutboxTenantIsolationTests : IntegrationTestBase
{
    [Fact]
    public async Task Outbox_Should_Isolate_Tenants_During_Dispatch()
    {
        // Arrange
        RecordingHandler.Clear();

        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        // Create Tenant A message
        using (var scope = ServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.OutboxMessages.Add(new OutboxMessage(new TestTenantIsolationDomainEvent(Guid.NewGuid()), tenantA));
            await db.SaveChangesAsync();
        }

        // Create Tenant B message
        using (var scope = ServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.OutboxMessages.Add(new OutboxMessage(new TestTenantIsolationDomainEvent(Guid.NewGuid()), tenantB));
            await db.SaveChangesAsync();
        }

        // Act — manually process outbox
        using (var scope = ServiceProvider.CreateScope())
        {
            var processor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();

            await processor.ProcessOutboxMessages(CancellationToken.None);
        }

        // Assert
        Assert.Equal(2, RecordingHandler.Processed.Count);

        var processedTenantIds = RecordingHandler.Processed
            .Select(x => x.TenantId)
            .Distinct()
            .ToList();

        Assert.Contains(tenantA, processedTenantIds);
        Assert.Contains(tenantB, processedTenantIds);
    }

}
