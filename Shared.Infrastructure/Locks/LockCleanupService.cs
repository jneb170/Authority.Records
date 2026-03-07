using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Audit;
using Shared.Infrastructure.Persistence;
using System.Text.Json;

namespace Shared.Infrastructure.Locks;

/// <summary>
/// Background service that releases stale locks on startup and periodically.
/// Handles records whose lock timer expired while the server was offline or
/// the browser session ended without a clean release.
/// </summary>
public sealed class LockCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly LockCleanupOptions _options;
    private readonly ILogger<LockCleanupService> _logger;

    // Synthetic event type name written to AuditTrailEntries for system-released locks.
    private const string SystemLockExpiredEvent = "SystemLockExpired";

    public LockCleanupService(
        IServiceScopeFactory scopeFactory,
        IOptions<LockCleanupOptions> options,
        ILogger<LockCleanupService> logger)
    {
        _scopeFactory  = scopeFactory;
        _options       = options.Value;
        _logger        = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run once immediately on startup to clear any locks left from a previous run.
        await ReleaseExpiredLocksAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_options.CheckInterval, stoppingToken);
            await ReleaseExpiredLocksAsync(stoppingToken);
        }
    }

    private async Task ReleaseExpiredLocksAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db     = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var cutoff = DateTime.UtcNow.Subtract(_options.LockTimeout);
            var now    = DateTime.UtcNow;

            // Collect expired lock records (with audit data) before releasing them.
            var expiredIncidents = await CollectExpiredIncidentLocksAsync(db, cutoff, ct);
            var expiredArrests   = await CollectExpiredArrestLocksAsync(db, cutoff, ct);
            var expiredCitations = await CollectExpiredCitationLocksAsync(db, cutoff, ct);

            var total = expiredIncidents.Count + expiredArrests.Count + expiredCitations.Count;
            if (total == 0) return;

            // Release locks in entity and read-model tables.
            await ReleaseEntityLocksAsync(db, expiredIncidents.Select(x => x.Id).ToList(), "Incidents", ct);
            await ReleaseEntityLocksAsync(db, expiredArrests.Select(x => x.Id).ToList(), "Arrests", ct);
            await ReleaseEntityLocksAsync(db, expiredCitations.Select(x => x.Id).ToList(), "Citations", ct);
            await ReleaseReadModelLocksAsync(
                db,
                expiredIncidents.Select(x => x.Id).ToList(),
                expiredArrests.Select(x => x.Id).ToList(),
                expiredCitations.Select(x => x.Id).ToList(),
                ct);

            // Write one audit entry per released lock.
            // AppDbContext.SaveChangesAsync will not call CurrentTenantId when there
            // are no domain events, so this is safe from a background thread.
            var auditEntries = BuildAuditEntries(expiredIncidents, "Incident", now)
                .Concat(BuildAuditEntries(expiredArrests, "Arrest", now))
                .Concat(BuildAuditEntries(expiredCitations, "Citation", now));

            db.AuditTrailEntries.AddRange(auditEntries);
            await db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Released {Count} expired lock(s): {Incidents} incident(s), {Arrests} arrest(s), {Citations} citation(s).",
                total, expiredIncidents.Count, expiredArrests.Count, expiredCitations.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lock cleanup failed.");
        }
    }

    // -----------------------------------------------------------------------
    // Collect helpers — query entity tables before releasing so we have the
    // audit data (JurisdictionId, LockedByUserId, Version, LockedAtUtc).
    // IgnoreQueryFilters: no HTTP context in a background thread, so the global
    // tenant + soft-delete filters cannot be evaluated. Intentional — lock
    // cleanup must operate across all tenants and all record states.
    // -----------------------------------------------------------------------

    private static async Task<List<ExpiredLockRecord>> CollectExpiredIncidentLocksAsync(
        AppDbContext db, DateTime cutoff, CancellationToken ct) =>
        await db.Incidents
            .IgnoreQueryFilters()
            .Where(i => i.LockedAtUtc != null && i.LockedAtUtc < cutoff)
            .Select(i => new ExpiredLockRecord(i.Id, i.JurisdictionId, i.LockedByUserId!.Value, i.LockedAtUtc!.Value, i.Version))
            .ToListAsync(ct);

    private static async Task<List<ExpiredLockRecord>> CollectExpiredArrestLocksAsync(
        AppDbContext db, DateTime cutoff, CancellationToken ct) =>
        await db.Arrests
            .IgnoreQueryFilters()
            .Where(a => a.LockedAtUtc != null && a.LockedAtUtc < cutoff)
            .Select(a => new ExpiredLockRecord(a.Id, a.JurisdictionId, a.LockedByUserId!.Value, a.LockedAtUtc!.Value, a.Version))
            .ToListAsync(ct);

    private static async Task<List<ExpiredLockRecord>> CollectExpiredCitationLocksAsync(
        AppDbContext db, DateTime cutoff, CancellationToken ct) =>
        await db.Citations
            .IgnoreQueryFilters()
            .Where(c => c.LockedAtUtc != null && c.LockedAtUtc < cutoff)
            .Select(c => new ExpiredLockRecord(c.Id, c.JurisdictionId, c.LockedByUserId!.Value, c.LockedAtUtc!.Value, c.Version))
            .ToListAsync(ct);

    // -----------------------------------------------------------------------
    // Release helpers
    // -----------------------------------------------------------------------

    private static async Task ReleaseEntityLocksAsync(
        AppDbContext db, List<Guid> ids, string table, CancellationToken ct)
    {
        if (ids.Count == 0) return;

        switch (table)
        {
            case "Incidents":
                await db.Incidents.IgnoreQueryFilters()
                    .Where(i => ids.Contains(i.Id))
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(i => i.LockedByUserId, (Guid?)null)
                        .SetProperty(i => i.LockedAtUtc, (DateTime?)null), ct);
                break;
            case "Arrests":
                await db.Arrests.IgnoreQueryFilters()
                    .Where(a => ids.Contains(a.Id))
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(a => a.LockedByUserId, (Guid?)null)
                        .SetProperty(a => a.LockedAtUtc, (DateTime?)null), ct);
                break;
            case "Citations":
                await db.Citations.IgnoreQueryFilters()
                    .Where(c => ids.Contains(c.Id))
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(c => c.LockedByUserId, (Guid?)null)
                        .SetProperty(c => c.LockedAtUtc, (DateTime?)null), ct);
                break;
        }
    }

    private static async Task ReleaseReadModelLocksAsync(
        AppDbContext db,
        List<Guid> incidentIds,
        List<Guid> arrestIds,
        List<Guid> citationIds,
        CancellationToken ct)
    {
        if (incidentIds.Count > 0)
            await db.IncidentReadModels
                .Where(r => incidentIds.Contains(r.Id))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(r => r.IsLocked, false)
                    .SetProperty(r => r.LockedByUserId, (Guid?)null), ct);

        if (arrestIds.Count > 0)
            await db.ArrestReadModels
                .Where(r => arrestIds.Contains(r.Id))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(r => r.IsLocked, false)
                    .SetProperty(r => r.LockedByUserId, (Guid?)null), ct);

        if (citationIds.Count > 0)
            await db.CitationReadModels
                .Where(r => citationIds.Contains(r.Id))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(r => r.IsLocked, false)
                    .SetProperty(r => r.LockedByUserId, (Guid?)null), ct);
    }

    // -----------------------------------------------------------------------
    // Audit helpers
    // -----------------------------------------------------------------------

    private static IEnumerable<AuditTrailEntry> BuildAuditEntries(
        List<ExpiredLockRecord> records, string aggregateType, DateTime expiredAtUtc) =>
        records.Select(r =>
        {
            var payload = JsonSerializer.Serialize(new
            {
                AggregateType    = aggregateType,
                LockedByUserId   = r.LockedByUserId,
                LockedAtUtc      = r.LockedAtUtc,
                ExpiredAtUtc     = expiredAtUtc,
            });

            return AuditTrailEntry.Create(
                eventId:          Guid.NewGuid(),
                eventType:        SystemLockExpiredEvent,
                occurredOnUtc:    expiredAtUtc,
                jurisdictionId:   r.JurisdictionId,
                aggregateId:      r.Id,
                aggregateVersion: r.Version,
                payload:          payload);
        });

    // -----------------------------------------------------------------------
    // Internal projection for query results
    // -----------------------------------------------------------------------

    private sealed record ExpiredLockRecord(
        Guid     Id,
        Guid     JurisdictionId,
        Guid     LockedByUserId,
        DateTime LockedAtUtc,
        long     Version);
}

