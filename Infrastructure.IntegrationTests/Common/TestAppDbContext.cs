using Infrastructure.IntegrationTests.Outbox.Idempotency;
using Infrastructure.IntegrationTests.Outbox.RetryBehavior;
using Infrastructure.IntegrationTests.Outbox.TenantIsolation;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.DomainEvents;
using Shared.Infrastructure.Persistence;

namespace Infrastructure.IntegrationTests.Common;

internal sealed class TestAppDbContext : AppDbContext
{
    public TestAppDbContext(
        DbContextOptions<TestAppDbContext> options,
        ITenantProvider tenantProvider,
        IDomainEventDispatcher dispatcher)
        : base(options, tenantProvider, dispatcher)
    {
    }

    public DbSet<TestTenantIsolationAggregate> TestAggregates => Set<TestTenantIsolationAggregate>();
    public DbSet<FailingAggregate> FailingAggregates => Set<FailingAggregate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TestTenantIsolationAggregate>(builder =>
        {
            builder.HasKey(x => x.Id);
        });

        modelBuilder.Entity<TestIdempotencyAggregate>(builder =>
        {
            builder.HasKey(x => x.Id);
        });

        modelBuilder.Entity<FailingAggregate>(builder =>
        {
            builder.HasKey(x => x.Id);
        });
    }
}
