using Microsoft.EntityFrameworkCore;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.DomainEvents;
using Shared.Infrastructure.Persistence;

namespace Infrastructure.IntegrationTests.TestInfrastructure;

/// <summary>
/// Thin AppDbContext subclass kept so that tests can use DbContextOptions&lt;SqliteTestAppDbContext&gt;.
/// All SQLite-specific model overrides now live in AppDbContext.OnModelCreating, guarded by
/// Database.IsSqlite(), so this class no longer needs to add any.
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
}
