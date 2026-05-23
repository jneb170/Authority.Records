using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modules.Records.Domain.Common;
using Shared.Infrastructure.Audit;
using Shared.Infrastructure.Persistence;

namespace Shared.Infrastructure.Locks;

public sealed class LockCleanupService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly LockCleanupOptions _options;
    private readonly ILogger<LockCleanupService> _logger;

    public LockCleanupService(
        IServiceScopeFactory scopeFactory,
        IOptions<LockCleanupOptions> options,
        ILogger<LockCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task ReleaseExpiredLocksAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var now = DateTime.UtcNow;

        // The lock timeout is configured per agency (ConfigurationKeys.LockTimeoutSeconds),
        // so there is no single global cutoff. Load every agency's configured timeout, then
        // collect all currently-held locks and decide expiry per record against its own
        // agency's timeout. Agencies without a setting fall back to _options.LockTimeout
        // (whose default mirrors ConfigurationKeys.DefaultLockTimeoutSeconds).
        var timeouts = await LoadAgencyTimeoutsAsync(db, ct);
        TimeSpan TimeoutFor(Guid agencyId) =>
            timeouts.TryGetValue(agencyId, out var t) ? t : _options.LockTimeout;

        bool IsExpired(ExpiredLockRecord r) => r.LockedAtUtc.Add(TimeoutFor(r.AgencyId)) <= now;

        // Collect held locks (with audit data), then filter to the expired ones in memory.
        var expiredNames = (await CollectHeldNameLocksAsync(db, ct)).Where(IsExpired).ToList();
        var expiredIncidents = (await CollectHeldIncidentLocksAsync(db, ct)).Where(IsExpired).ToList();
        var expiredArrests = (await CollectHeldArrestLocksAsync(db, ct)).Where(IsExpired).ToList();
        var expiredCitations = (await CollectHeldCitationLocksAsync(db, ct)).Where(IsExpired).ToList();
        var expiredLocations = (await CollectHeldLocationLocksAsync(db, ct)).Where(IsExpired).ToList();

        var total = expiredNames.Count + expiredIncidents.Count + expiredArrests.Count
            + expiredCitations.Count + expiredLocations.Count;
        if (total == 0)
            return;

        // Release locks in entity and read-model tables.
        await ReleaseEntityLocksAsync(db, expiredNames.Select(x => x.Id).ToList(), "Names", ct);
        await ReleaseEntityLocksAsync(db, expiredIncidents.Select(x => x.Id).ToList(), "Incidents", ct);
        await ReleaseEntityLocksAsync(db, expiredArrests.Select(x => x.Id).ToList(), "Arrests", ct);
        await ReleaseEntityLocksAsync(db, expiredCitations.Select(x => x.Id).ToList(), "Citations", ct);
        await ReleaseEntityLocksAsync(db, expiredLocations.Select(x => x.Id).ToList(), "Locations", ct);
        await ReleaseReadModelLocksAsync(
            db,
            expiredNames.Select(x => x.Id).ToList(),
            expiredIncidents.Select(x => x.Id).ToList(),
            expiredArrests.Select(x => x.Id).ToList(),
            expiredCitations.Select(x => x.Id).ToList(),
            expiredLocations.Select(x => x.Id).ToList(),
            ct);

        // Write one audit entry per released lock.
        var auditEntries = BuildAuditEntries(expiredNames, "Name", now)
            .Concat(BuildAuditEntries(expiredIncidents, "Incident", now))
            .Concat(BuildAuditEntries(expiredArrests, "Arrest", now))
            .Concat(BuildAuditEntries(expiredCitations, "Citation", now))
            .Concat(BuildAuditEntries(expiredLocations, "Location", now));

        db.AuditLogReadModels.AddRange(auditEntries);
        await db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Released {Count} expired lock(s): {Names} name(s), {Incidents} incident(s), {Arrests} arrest(s), {Citations} citation(s), {Locations} location(s).",
            total, expiredNames.Count, expiredIncidents.Count, expiredArrests.Count, expiredCitations.Count, expiredLocations.Count);
    }

    // -----------------------------------------------------------------------
    // Per-agency timeout map. Read across all tenants (IgnoreQueryFilters, no
    // HTTP context here), excluding soft-deleted config rows so a deleted
    // setting reverts the agency to the default rather than a stale value.
    // -----------------------------------------------------------------------

    private static async Task<Dictionary<Guid, TimeSpan>> LoadAgencyTimeoutsAsync(
        AppDbContext db, CancellationToken ct)
    {
        var rows = await db.AgencyConfigurations
            .IgnoreQueryFilters()
            .Where(c => c.Key == ConfigurationKeys.LockTimeoutSeconds && !c.IsDeleted)
            .Select(c => new { c.AgencyId, c.Value })
            .ToListAsync(ct);

        var map = new Dictionary<Guid, TimeSpan>();
        foreach (var row in rows)
            map[row.AgencyId] = LockTimeout.FromConfigValue(row.Value);
        return map;
    }

    // -----------------------------------------------------------------------
    // Collect helpers — query entity tables for currently-held locks so we have
    // the audit data (AgencyId, JurisdictionId, LockedByUserId, Version,
    // LockedAtUtc). Expiry is decided per record by the caller using the owning
    // agency's configured timeout.
    // IgnoreQueryFilters: no HTTP context in a background thread, so the global
    // tenant + soft-delete filters cannot be evaluated. Intentional — lock
    // cleanup must operate across all tenants and all record states.
    // -----------------------------------------------------------------------

    private static async Task<List<ExpiredLockRecord>> CollectHeldNameLocksAsync(
        AppDbContext db, CancellationToken ct) =>
        await db.Names
            .IgnoreQueryFilters()
            .Where(n => n.LockedAtUtc != null)
            .Select(n => new ExpiredLockRecord(n.Id, n.AgencyId, n.JurisdictionId, n.LockedByUserId!.Value, n.LockedAtUtc!.Value, n.Version))
            .ToListAsync(ct);

    private static async Task<List<ExpiredLockRecord>> CollectHeldIncidentLocksAsync(
        AppDbContext db, CancellationToken ct) =>
        await db.Incidents
            .IgnoreQueryFilters()
            .Where(i => i.LockedAtUtc != null)
            .Select(i => new ExpiredLockRecord(i.Id, i.AgencyId, i.JurisdictionId, i.LockedByUserId!.Value, i.LockedAtUtc!.Value, i.Version))
            .ToListAsync(ct);

    private static async Task<List<ExpiredLockRecord>> CollectHeldArrestLocksAsync(
        AppDbContext db, CancellationToken ct) =>
        await db.Arrests
            .IgnoreQueryFilters()
            .Where(a => a.LockedAtUtc != null)
            .Select(a => new ExpiredLockRecord(a.Id, a.AgencyId, a.JurisdictionId, a.LockedByUserId!.Value, a.LockedAtUtc!.Value, a.Version))
            .ToListAsync(ct);

    private static async Task<List<ExpiredLockRecord>> CollectHeldCitationLocksAsync(
        AppDbContext db, CancellationToken ct) =>
        await db.Citations
            .IgnoreQueryFilters()
            .Where(c => c.LockedAtUtc != null)
            .Select(c => new ExpiredLockRecord(c.Id, c.AgencyId, c.JurisdictionId, c.LockedByUserId!.Value, c.LockedAtUtc!.Value, c.Version))
            .ToListAsync(ct);

    // Location is the shared Master Location Index — jurisdiction-scoped, with no permanent AgencyId.
    // The lock owner's agency is captured on the lock (LockedByAgencyId) so its configured timeout
    // governs expiry. A null (e.g. a lock taken before this column existed) maps to Guid.Empty, which
    // is absent from the timeout map and therefore falls back to the system default.
    private static async Task<List<ExpiredLockRecord>> CollectHeldLocationLocksAsync(
        AppDbContext db, CancellationToken ct) =>
        await db.Locations
            .IgnoreQueryFilters()
            .Where(l => l.LockedAtUtc != null)
            .Select(l => new ExpiredLockRecord(l.Id, l.LockedByAgencyId ?? Guid.Empty, l.JurisdictionId, l.LockedByUserId!.Value, l.LockedAtUtc!.Value, l.Version))
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
            case "Names":
                await db.Names.IgnoreQueryFilters()
                    .Where(n => ids.Contains(n.Id))
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(n => n.LockedByUserId, (Guid?)null)
                        .SetProperty(n => n.LockedAtUtc, (DateTime?)null), ct);
                break;
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
            case "Locations":
                await db.Locations.IgnoreQueryFilters()
                    .Where(l => ids.Contains(l.Id))
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(l => l.LockedByUserId, (Guid?)null)
                        .SetProperty(l => l.LockedAtUtc, (DateTime?)null)
                        .SetProperty(l => l.LockedByAgencyId, (Guid?)null), ct);
                break;
        }
    }

    private static async Task ReleaseReadModelLocksAsync(
        AppDbContext db,
        List<Guid> nameIds,
        List<Guid> incidentIds,
        List<Guid> arrestIds,
        List<Guid> citationIds,
        List<Guid> locationIds,
        CancellationToken ct)
    {
        if (nameIds.Count > 0)
            await db.NameReadModels
                .Where(r => nameIds.Contains(r.Id))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(r => r.IsLocked, false)
                    .SetProperty(r => r.LockedByUserId, (Guid?)null), ct);

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

        if (locationIds.Count > 0)
            await db.LocationReadModels
                .Where(r => locationIds.Contains(r.Id))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(r => r.IsLocked, false)
                    .SetProperty(r => r.LockedByUserId, (Guid?)null), ct);
    }

    // -----------------------------------------------------------------------
    // Audit helpers
    // -----------------------------------------------------------------------

    private static IEnumerable<Modules.Records.Application.ReadModels.AuditLogReadModel> BuildAuditEntries(
        List<ExpiredLockRecord> records, string aggregateType, DateTime expiredAtUtc) =>
        records.Select(r =>
            AuditLogEntryFactory.CreateSystemLockExpired(
                r.JurisdictionId,
                r.Id,
                r.Version,
                aggregateType,
                expiredAtUtc,
                r.LockedByUserId,
                r.LockedAtUtc));

    // -----------------------------------------------------------------------
    // Internal projection for query results
    // -----------------------------------------------------------------------

    private sealed record ExpiredLockRecord(
        Guid Id,
        Guid AgencyId,
        Guid JurisdictionId,
        Guid LockedByUserId,
        DateTime LockedAtUtc,
        long Version);
}

