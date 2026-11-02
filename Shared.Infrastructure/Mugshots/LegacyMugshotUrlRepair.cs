using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.Records.Application.Mugshots;
using Shared.Infrastructure.Persistence;

namespace Shared.Infrastructure.Mugshots;

/// <summary>
/// One-time, idempotent repair for mugshots uploaded while images were served as static
/// files. Rewrites their stored URL (<c>/uploads/mugshots/...</c>, no longer served) to the
/// authorized endpoint (<c>/mugshots/{id}</c>) on the aggregate, its read model, and the
/// Name/Arrest read models that denormalize the primary mugshot URL.
/// </summary>
/// <remarks>
/// Uses set-based updates that bypass the change tracker (no audit, outbox or domain
/// events): this is a data-format migration, not a change to the record. Query filters are
/// ignored because it runs at startup, outside any tenant. Once no legacy URLs remain,
/// each boot costs a single query.
/// </remarks>
public static class LegacyMugshotUrlRepair
{
    public static async Task RepairAsync(
        IServiceProvider services,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        // Best-effort: a startup data repair must never stop the host from booting.
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var repaired = await RepairCoreAsync(db, cancellationToken);

            if (repaired > 0)
            {
                logger.LogWarning(
                    "Rewrote {Count} legacy mugshot URL(s) from {LegacyPrefix}... to the authorized mugshot endpoint.",
                    repaired,
                    MugshotUrls.LegacyUrlPrefix);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Legacy mugshot URL repair failed. Continuing startup; affected mugshots will not " +
                "display until the repair succeeds on a later boot.");
        }
    }

    /// <summary>Returns the number of mugshots whose URL was rewritten.</summary>
    internal static async Task<int> RepairCoreAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        var legacy = await db.Mugshots
            .IgnoreQueryFilters()
            .Where(m => m.PublicUrl.StartsWith(MugshotUrls.LegacyUrlPrefix))
            .Select(m => new { m.Id, m.PublicUrl })
            .ToListAsync(cancellationToken);

        foreach (var mugshot in legacy)
        {
            var oldUrl = mugshot.PublicUrl;
            var newUrl = MugshotUrls.For(mugshot.Id);

            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

            await db.MugshotReadModels.IgnoreQueryFilters()
                .Where(m => m.Id == mugshot.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.PublicUrl, newUrl), cancellationToken);

            await db.NameReadModels.IgnoreQueryFilters()
                .Where(n => n.PrimaryMugshotUrl == oldUrl)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.PrimaryMugshotUrl, newUrl), cancellationToken);

            await db.ArrestReadModels.IgnoreQueryFilters()
                .Where(a => a.PrimaryMugshotUrl == oldUrl)
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.PrimaryMugshotUrl, newUrl), cancellationToken);

            // The aggregate last: it is what selects legacy rows, so if anything above
            // fails this mugshot is picked up again next boot.
            await db.Mugshots.IgnoreQueryFilters()
                .Where(m => m.Id == mugshot.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.PublicUrl, newUrl), cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }

        return legacy.Count;
    }
}
