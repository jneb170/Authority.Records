using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.Records.Application.Admin.Commands.RebuildReadModels;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;

namespace Shared.Infrastructure.Persistence;

/// <summary>
/// One-time, idempotent repair for records created during the window when SQLite
/// generated RecordNumber via ABS(RANDOM()) (huge ~64-bit values surfaced in URLs).
/// Renumbers those outliers back into the short sequential range that matches the
/// original SqlServer identity, then rebuilds read models per affected jurisdiction
/// so every denormalized RecordNumber copy is brought back in sync.
///
/// SQLite only. After the first run there are no outliers left, so subsequent
/// boots are a no-op (a single cheap query per aggregate type).
/// </summary>
public static class SqliteRecordNumberRepair
{
    public static async Task RepairAsync(
        IServiceProvider services,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        // Best-effort, like the demo seeder: a startup data repair must NEVER crash or
        // block the host. Any failure is logged and swallowed so the app still boots;
        // the worst case is that legacy random RecordNumbers stick around until next try.
        try
        {
            await RepairCoreAsync(services, logger, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "SQLite RecordNumber repair failed. Continuing startup; legacy random " +
                "RecordNumbers (if any) were left unchanged and will be retried next boot.");
        }
    }

    private static async Task RepairCoreAsync(
        IServiceProvider services,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var db = sp.GetRequiredService<AppDbContext>();

        if (!db.Database.IsSqlite())
            return;

        var affectedJurisdictions = new HashSet<Guid>();
        affectedJurisdictions.UnionWith(await RenumberAsync<Incident>(db, seed: 10000, logger, cancellationToken));
        affectedJurisdictions.UnionWith(await RenumberAsync<Arrest>(db, seed: 10000, logger, cancellationToken));
        affectedJurisdictions.UnionWith(await RenumberAsync<Citation>(db, seed: 10000, logger, cancellationToken));
        affectedJurisdictions.UnionWith(await RenumberAsync<Name>(db, seed: 10000, logger, cancellationToken));
        affectedJurisdictions.UnionWith(await RenumberAsync<Location>(db, seed: 20000, logger, cancellationToken));

        if (affectedJurisdictions.Count == 0)
            return;

        await db.SaveChangesAsync(cancellationToken);

        // Read models (and the RecordNumbers they denormalize from other aggregates)
        // are rebuilt wholesale per jurisdiction so every copy reflects the new numbers.
        var tenantProvider = sp.GetRequiredService<ITenantProvider>();

        // Invoke the rebuild handler directly instead of via MediatR. At startup there is
        // no Razor circuit, so the request-pipeline behaviors — notably the demo
        // write-guard, which reads the Blazor auth state — would throw. This is an
        // internal system operation, so bypassing those behaviors is correct.
        var rebuildHandler = new RebuildReadModelsHandler(db, tenantProvider);

        foreach (var jurisdictionId in affectedJurisdictions)
        {
            tenantProvider.SetJurisdictionId(jurisdictionId);
            await rebuildHandler.Handle(new RebuildReadModelsCommand(), cancellationToken);
        }

        logger.LogWarning(
            "SQLite RecordNumber repair renumbered legacy random RecordNumbers and rebuilt " +
            "read models for {JurisdictionCount} jurisdiction(s).",
            affectedJurisdictions.Count);
    }

    private static async Task<HashSet<Guid>> RenumberAsync<T>(
        AppDbContext db,
        long seed,
        ILogger logger,
        CancellationToken cancellationToken) where T : class
    {
        var affected = new HashSet<Guid>();

        var outliers = await db.Set<T>()
            .IgnoreQueryFilters()
            .Where(x => EF.Property<long>(x, "RecordNumber") >= AppDbContext.RandomRecordNumberThreshold)
            .OrderBy(x => EF.Property<DateTime>(x, "CreatedAt"))
            .ThenBy(x => EF.Property<Guid>(x, "Id"))
            .ToListAsync(cancellationToken);

        if (outliers.Count == 0)
            return affected;

        var max = await db.Set<T>()
            .IgnoreQueryFilters()
            .Where(x => EF.Property<long>(x, "RecordNumber") < AppDbContext.RandomRecordNumberThreshold)
            .Select(x => (long?)EF.Property<long>(x, "RecordNumber"))
            .MaxAsync(cancellationToken) ?? (seed - 1);

        foreach (var entity in outliers)
        {
            var entry = db.Entry(entity);
            var oldNumber = (long)entry.Property("RecordNumber").CurrentValue!;
            var newNumber = ++max;

            entry.Property("RecordNumber").CurrentValue = newNumber;
            affected.Add((Guid)entry.Property("JurisdictionId").CurrentValue!);

            logger.LogWarning(
                "Renumbering {EntityType} {Id}: RecordNumber {OldNumber} -> {NewNumber}.",
                typeof(T).Name,
                entry.Property("Id").CurrentValue,
                oldNumber,
                newNumber);
        }

        return affected;
    }
}
