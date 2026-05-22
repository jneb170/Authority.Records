using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.DomainEvents;
using Shared.Infrastructure;
using Shared.Infrastructure.Persistence;

namespace Authority.Records.DataMigration;

/// <summary>
/// One-off utility that copies all business data from the production SQL Server
/// database into fresh SQLite database files (app.db + auth.db), reusing the app's
/// own EF Core model so provider quirks (RecordNumber defaults, rowversion → BLOB,
/// datetime2 → TEXT) are handled exactly as the running app handles them.
///
/// Read models are copied as-is; the running app's read-model rebuild is idempotent,
/// so a later rebuild simply upserts. The transient outbox / dead-letter tables are
/// skipped. Foreign keys are disabled on the SQLite side during the copy so table
/// order does not matter.
/// </summary>
internal static class Program
{
    // Transient infrastructure tables — not business data, regenerated at runtime.
    private static readonly HashSet<string> SkipTables = new(StringComparer.Ordinal)
    {
        "OutboxMessage",
        "DeadLetterMessage",
    };

    private static async Task<int> Main(string[] args)
    {
        string? source = GetArg(args, "--source") ?? Environment.GetEnvironmentVariable("SOURCE_SQLSERVER_CONNECTION");
        string outDir = Path.GetFullPath(GetArg(args, "--out") ?? "out");

        if (string.IsNullOrWhiteSpace(source))
        {
            Console.Error.WriteLine(
                "Missing source connection string.\n" +
                "Usage: dotnet run -- --source \"<SQL Server connection string>\" [--out <dir>]\n" +
                "   or: set SOURCE_SQLSERVER_CONNECTION and run with no --source.");
            return 1;
        }

        Directory.CreateDirectory(outDir);
        var appDbPath = Path.Combine(outDir, "app.db");
        var authDbPath = Path.Combine(outDir, "auth.db");
        DeleteSqliteFiles(appDbPath);
        DeleteSqliteFiles(authDbPath);

        var sourceCfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DefaultDatabaseProvider"] = "SqlServer",
                ["ConnectionStrings:DefaultConnection"] = source,
            })
            .Build();

        var targetCfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DefaultDatabaseProvider"] = "Sqlite",
                ["ConnectionStrings:SqliteAppConnection"] = $"Data Source={appDbPath};Cache=Shared;Foreign Keys=True",
                ["ConnectionStrings:SqliteAuthConnection"] = $"Data Source={authDbPath};Cache=Shared;Foreign Keys=True",
            })
            .Build();

        try
        {
            Console.WriteLine($"Output directory: {outDir}");
            Console.WriteLine("Building fresh SQLite schema (app.db, auth.db)...");

            await using (var targetApp = CreateApp(DatabaseProvider.Sqlite, targetCfg))
            await using (var targetAuth = CreateAuth(DatabaseProvider.Sqlite, targetCfg))
            {
                await targetApp.Database.MigrateAsync();
                await targetAuth.Database.MigrateAsync();
            }

            Console.WriteLine("\nCopying AuthDbContext (jurisdictions, agencies, users, roles)...");
            await using (var srcAuth = CreateAuth(DatabaseProvider.SqlServer, sourceCfg))
            await using (var dstAuth = CreateAuth(DatabaseProvider.Sqlite, targetCfg))
            {
                await CopyContextAsync(srcAuth, dstAuth);
            }

            Console.WriteLine("\nCopying AppDbContext (records, links, config, read models)...");
            await using (var srcApp = CreateApp(DatabaseProvider.SqlServer, sourceCfg))
            await using (var dstApp = CreateApp(DatabaseProvider.Sqlite, targetCfg))
            {
                await CopyContextAsync(srcApp, dstApp);
            }

            Console.WriteLine($"\nDone. SQLite files written to:\n  {appDbPath}\n  {authDbPath}");
            Console.WriteLine("Upload both to D:\\home\\site\\data\\ on the Azure App Service (Kudu).");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"\nFAILED: {ex.GetType().Name}: {ex.Message}");
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static async Task CopyContextAsync(DbContext src, DbContext dst)
    {
        await dst.Database.OpenConnectionAsync();
        try
        {
            SetForeignKeys(dst, enabled: false);

            foreach (var entityType in src.Model.GetEntityTypes())
            {
                if (entityType.IsOwned()) continue;
                if (entityType.GetTableName() is null) continue;
                if (entityType.FindPrimaryKey() is null) continue;

                var clrType = entityType.ClrType;
                if (SkipTables.Contains(clrType.Name))
                {
                    Console.WriteLine($"  {clrType.Name,-34} {"(skipped)",10}");
                    continue;
                }

                var count = await Copier.CopyAsync(clrType, src, dst);
                Console.WriteLine($"  {clrType.Name,-34} {count,10}");
            }
        }
        finally
        {
            SetForeignKeys(dst, enabled: true);
            Checkpoint(dst);
            await dst.Database.CloseConnectionAsync();
        }
    }

    // Fold the write-ahead log back into the main .db file and truncate it, so the
    // resulting app.db / auth.db are self-contained single files that can be uploaded
    // on their own (no -wal / -shm sidecars required).
    private static void Checkpoint(DbContext ctx)
    {
        var conn = ctx.Database.GetDbConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "PRAGMA wal_checkpoint(TRUNCATE);";
        cmd.ExecuteNonQuery();
    }

    private static void SetForeignKeys(DbContext ctx, bool enabled)
    {
        var conn = ctx.Database.GetDbConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"PRAGMA foreign_keys = {(enabled ? "ON" : "OFF")};";
        cmd.ExecuteNonQuery();
    }

    private static AppDbContext CreateApp(DatabaseProvider provider, IConfiguration cfg)
    {
        var builder = new DbContextOptionsBuilder<AppDbContext>();
        DependencyInjection.ConfigureProvider(builder, provider, isAuth: false, cfg);
        return new AppDbContext(builder.Options, new StubTenantProvider(), new NoOpDomainEventDispatcher());
    }

    private static AuthDbContext CreateAuth(DatabaseProvider provider, IConfiguration cfg)
    {
        var builder = new DbContextOptionsBuilder<AuthDbContext>();
        DependencyInjection.ConfigureProvider(builder, provider, isAuth: true, cfg);
        return new AuthDbContext(builder.Options);
    }

    private static string? GetArg(string[] args, string name)
    {
        for (var i = 0; i < args.Length - 1; i++)
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        return null;
    }

    private static void DeleteSqliteFiles(string dbPath)
    {
        foreach (var suffix in new[] { "", "-wal", "-shm", "-journal" })
        {
            var path = dbPath + suffix;
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}

internal static class Copier
{
    public static Task<int> CopyAsync(Type entityType, DbContext src, DbContext dst)
    {
        var method = typeof(Copier)
            .GetMethod(nameof(CopyGenericAsync), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(entityType);
        return (Task<int>)method.Invoke(null, new object[] { src, dst })!;
    }

    private static async Task<int> CopyGenericAsync<T>(DbContext src, DbContext dst) where T : class
    {
        var rows = await src.Set<T>().IgnoreQueryFilters().AsNoTracking().ToListAsync();
        if (rows.Count > 0)
        {
            dst.Set<T>().AddRange(rows);
            await dst.SaveChangesAsync();
            dst.ChangeTracker.Clear();
        }
        return rows.Count;
    }
}

/// <summary>No tenant context — the copy reads every row via IgnoreQueryFilters().</summary>
internal sealed class StubTenantProvider : ITenantProvider
{
    public Guid GetJurisdictionId() => Guid.Empty;
    public Guid GetAgencyId() => Guid.Empty;
    public Guid GetUserId() => Guid.Empty;
    public void SetJurisdictionId(Guid jurisdictionId) { }
}

/// <summary>Materialized entities raise no domain events, so this is never invoked.</summary>
internal sealed class NoOpDomainEventDispatcher : IDomainEventDispatcher
{
    public Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
