using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Outbox;
using Infrastructure.IntegrationTests.Common;
using Microsoft.Extensions.DependencyInjection;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Shared.Infrastructure.DomainEvents;
using Shared.Infrastructure.Outbox;
using Shared.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.IntegrationTests.Outbox.TenantIsolation
{
    public class OutboxTenantIsolationTests : IntegrationTestBase
    {
        [Fact]
        public async Task Outbox_Should_Isolate_Tenants_During_Dispatch()
        {
            // Arrange
            RecordingHandler.Clear();

            var tenantA = Guid.NewGuid();
            var tenantB = Guid.NewGuid();


            // Create Tenant A aggregate
            using (var scope = ServiceProvider.CreateScope())
            {
                var tenantProvider = (TestTenantProvider)scope.ServiceProvider.GetRequiredService<ITenantProvider>();
                tenantProvider.SetJurisdictionId(tenantA);

                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                db.Add(new TestTenantIsolationAggregate(Guid.NewGuid(), tenantA));
                await db.SaveChangesAsync();
            }

            // Create Tenant B aggregate
            using (var scope = ServiceProvider.CreateScope())
            {
                var tenantProvider = (TestTenantProvider)scope.ServiceProvider.GetRequiredService<ITenantProvider>();
                tenantProvider.SetJurisdictionId(tenantB);

                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                db.Add(new TestTenantIsolationAggregate(Guid.NewGuid(), tenantB));
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
}
