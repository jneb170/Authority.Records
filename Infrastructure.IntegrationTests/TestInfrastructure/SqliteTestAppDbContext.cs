using Microsoft.EntityFrameworkCore;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Entities;
using Shared.Infrastructure.Persistence;

namespace Infrastructure.IntegrationTests.TestInfrastructure;

/// <summary>
/// AppDbContext variant for SQLite integration tests.
/// Overrides SQL Server-specific configurations that are incompatible with SQLite.
/// </summary>
public sealed class SqliteTestAppDbContext : AppDbContext
{
    public SqliteTestAppDbContext(
        DbContextOptions<SqliteTestAppDbContext> options,
        ITenantProvider tenantProvider,
        IDomainEventDispatcher dispatcher)
        : base(options, tenantProvider, dispatcher)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // SQLite does not support SQL Server IDENTITY columns on non-PK columns.
        // Use ABS(RANDOM()) as a unique stand-in for tests.
        modelBuilder.Entity<Incident>()
            .Property(x => x.RecordNumber)
            .HasDefaultValueSql("ABS(RANDOM())");

        modelBuilder.Entity<Arrest>()
            .Property(x => x.RecordNumber)
            .HasDefaultValueSql("ABS(RANDOM())");

        modelBuilder.Entity<Citation>()
            .Property(x => x.RecordNumber)
            .HasDefaultValueSql("ABS(RANDOM())");

        modelBuilder.Entity<Name>()
            .Property(x => x.RecordNumber)
            .HasDefaultValueSql("ABS(RANDOM())");

        modelBuilder.Entity<Location>()
            .Property(x => x.RecordNumber)
            .HasDefaultValueSql("ABS(RANDOM())");
    }
}
