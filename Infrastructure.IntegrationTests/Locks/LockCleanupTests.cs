using Infrastructure.IntegrationTests.Common;
using Infrastructure.IntegrationTests.TestInfrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Entities;
using Shared.Infrastructure.Locks;
using Shared.Infrastructure.Persistence;

namespace Infrastructure.IntegrationTests.Locks;

/// <summary>
/// Exercises the real <see cref="LockCleanupService"/> against an in-memory SQLite database to
/// confirm that an expired record lock is actually released, and that the expiry is governed by
/// the owning agency's configured <see cref="ConfigurationKeys.LockTimeoutSeconds"/> (falling
/// back to the system default when unset).
/// </summary>
public sealed class LockCleanupTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _provider;
    private readonly Guid _jurisdictionId = Guid.NewGuid();

    public LockCleanupTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext, SqliteTestAppDbContext>(o => o.UseSqlite(_connection));
        services.AddScoped<ITenantProvider>(_ => new TestTenantProvider(_jurisdictionId));
        services.AddScoped<IDomainEventDispatcher, TestDomainEventDispatcher>();
        services.AddSingleton<IOptions<LockCleanupOptions>>(Options.Create(new LockCleanupOptions()));
        services.AddSingleton<LockCleanupService>();

        _provider = services.BuildServiceProvider();

        using var scope = _provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
    }

    [Fact]
    public async Task ReleaseExpiredLocks_HonoursPerAgencyTimeout()
    {
        var agencyConfigured = Guid.NewGuid();   // LockTimeoutSeconds = 20
        var agencyDefault = Guid.NewGuid();       // no setting -> default (600s)
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        Guid configuredId, defaultId;

        using (var scope = _provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.AgencyConfigurations.Add(new AgencyConfiguration(
                _jurisdictionId, agencyConfigured, ConfigurationKeys.LockTimeoutSeconds, "20"));

            var configured = new Citation(_jurisdictionId, agencyConfigured, "Configured agency", now, "CT-CFG");
            var @default = new Citation(_jurisdictionId, agencyDefault, "Default agency", now, "CT-DEF");
            db.Citations.Add(configured);
            db.Citations.Add(@default);

            db.CitationReadModels.Add(LockedReadModel(configured.Id, agencyConfigured, userId, now, 5001));
            db.CitationReadModels.Add(LockedReadModel(@default.Id, agencyDefault, userId, now, 5002));

            await db.SaveChangesAsync();
            configuredId = configured.Id;
            defaultId = @default.Id;

            // Both locks were taken 60 seconds ago. That is past the configured agency's 20s
            // timeout, but well within the default agency's 600s timeout.
            await BackdateLockAsync(db, userId, now.AddSeconds(-60), configuredId, defaultId);
        }

        await _provider.GetRequiredService<LockCleanupService>()
            .ReleaseExpiredLocksAsync(CancellationToken.None);

        using (var scope = _provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var configured = await db.Citations.IgnoreQueryFilters().SingleAsync(c => c.Id == configuredId);
            var @default = await db.Citations.IgnoreQueryFilters().SingleAsync(c => c.Id == defaultId);

            // 60s old > 20s configured timeout -> released.
            Assert.False(configured.IsLocked);
            Assert.Null(configured.LockedByUserId);
            Assert.Null(configured.LockedAtUtc);

            // 60s old < 600s default timeout -> still held.
            Assert.True(@default.IsLocked);
            Assert.Equal(userId, @default.LockedByUserId);

            // Read-model lock flag follows the entity for the released record.
            var configuredRm = await db.CitationReadModels.IgnoreQueryFilters().SingleAsync(r => r.Id == configuredId);
            var defaultRm = await db.CitationReadModels.IgnoreQueryFilters().SingleAsync(r => r.Id == defaultId);
            Assert.False(configuredRm.IsLocked);
            Assert.Null(configuredRm.LockedByUserId);
            Assert.True(defaultRm.IsLocked);
        }
    }

    [Fact]
    public async Task ReleaseExpiredLocks_LeavesLockHeldWithinConfiguredTimeout()
    {
        var agencyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        Guid citationId;

        using (var scope = _provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.AgencyConfigurations.Add(new AgencyConfiguration(
                _jurisdictionId, agencyId, ConfigurationKeys.LockTimeoutSeconds, "20"));

            var citation = new Citation(_jurisdictionId, agencyId, "Fresh lock", now, "CT-FRESH");
            db.Citations.Add(citation);
            await db.SaveChangesAsync();
            citationId = citation.Id;

            // Locked only 5 seconds ago — within the 20s timeout.
            await BackdateLockAsync(db, userId, now.AddSeconds(-5), citationId);
        }

        await _provider.GetRequiredService<LockCleanupService>()
            .ReleaseExpiredLocksAsync(CancellationToken.None);

        using (var scope = _provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var citation = await db.Citations.IgnoreQueryFilters().SingleAsync(c => c.Id == citationId);

            Assert.True(citation.IsLocked);
            Assert.Equal(userId, citation.LockedByUserId);
        }
    }

    private static Task BackdateLockAsync(AppDbContext db, Guid userId, DateTime lockedAtUtc, params Guid[] ids) =>
        db.Citations.IgnoreQueryFilters()
            .Where(c => ids.Contains(c.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.LockedByUserId, userId)
                .SetProperty(c => c.LockedAtUtc, lockedAtUtc));

    private CitationReadModel LockedReadModel(Guid id, Guid agencyId, Guid userId, DateTime now, long recordNumber)
    {
        var rm = CitationReadModel.Create(id, recordNumber, _jurisdictionId, agencyId, "desc", now, now, userId);
        rm.ApplyLockAcquired(userId);
        return rm;
    }

    public void Dispose()
    {
        _provider.Dispose();
        _connection.Dispose();
    }
}
