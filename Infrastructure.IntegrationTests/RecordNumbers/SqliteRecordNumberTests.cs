using Infrastructure.IntegrationTests.Common;
using Infrastructure.IntegrationTests.TestInfrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Domain.Entities;

namespace Infrastructure.IntegrationTests.RecordNumbers;

/// <summary>
/// SQLite has no IDENTITY columns, so RecordNumber is assigned in
/// AppDbContext.SaveChanges. These tests lock in the short, sequential,
/// URL-friendly numbering that mirrors the SqlServer identity (and replaces the
/// original cutover's ABS(RANDOM()) values like 7036875833685180546).
/// </summary>
public sealed class SqliteRecordNumberTests : IDisposable
{
    private const long CitationSeed = 10000;

    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<SqliteTestAppDbContext> _options;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _agencyId = Guid.NewGuid();

    public SqliteRecordNumberTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<SqliteTestAppDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    [Fact]
    public async Task NewRecords_GetShortSequentialRecordNumbers_StartingAtSeed()
    {
        await using var context = CreateContext();

        var first = new Citation(_tenantId, _agencyId, "First", new DateTime(2026, 01, 01), "CT-1");
        var second = new Citation(_tenantId, _agencyId, "Second", new DateTime(2026, 01, 02), "CT-2");

        context.Citations.Add(first);
        await context.SaveChangesAsync();
        context.Citations.Add(second);
        await context.SaveChangesAsync();

        Assert.Equal(CitationSeed, first.RecordNumber);
        Assert.Equal(CitationSeed + 1, second.RecordNumber);
    }

    [Fact]
    public async Task LegacyRandomOutlier_DoesNotPoison_NextSequentialNumber()
    {
        await using var context = CreateContext();

        var normal = new Citation(_tenantId, _agencyId, "Normal", new DateTime(2026, 01, 01), "CT-1");
        context.Citations.Add(normal);
        await context.SaveChangesAsync(); // → 10000

        // Simulate a record created during the ABS(RANDOM()) window: a positive
        // RecordNumber is preserved as-is (assignment only fills values <= 0).
        var outlier = new Citation(_tenantId, _agencyId, "Outlier", new DateTime(2026, 01, 02), "CT-2");
        context.Citations.Add(outlier);
        context.Entry(outlier).Property("RecordNumber").CurrentValue = 7036875833685180546L;
        await context.SaveChangesAsync();

        var next = new Citation(_tenantId, _agencyId, "Next", new DateTime(2026, 01, 03), "CT-3");
        context.Citations.Add(next);
        await context.SaveChangesAsync();

        Assert.Equal(7036875833685180546L, outlier.RecordNumber);
        // Chains off the highest non-outlier number, not the random value.
        Assert.Equal(CitationSeed + 1, next.RecordNumber);
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    private SqliteTestAppDbContext CreateContext()
    {
        return new SqliteTestAppDbContext(
            _options,
            new TestTenantProvider(_tenantId, _agencyId),
            new TestDomainEventDispatcher());
    }
}
