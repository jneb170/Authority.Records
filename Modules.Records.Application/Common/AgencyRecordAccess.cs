using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Common;

namespace Modules.Records.Application.Common;

/// <summary>
/// Access checks for records that hang off an owner by (OwnerType, OwnerId) — narratives and
/// mugshots — so they inherit their owner's agency isolation (issue #118).
/// <para>
/// Incidents, arrests and citations belong to one agency; the agency query filter on
/// <see cref="IApplicationDbContext"/> hides another agency's, so an owner that can't be found
/// here is not accessible. Names (and anything else) are the shared master index and are
/// accessible to every agency in the jurisdiction.
/// </para>
/// </summary>
internal static class AgencyRecordAccess
{
    public static Task<bool> CanAccessOwnerAsync(
        IApplicationDbContext db,
        string ownerType,
        Guid ownerId,
        CancellationToken cancellationToken)
    {
        // Deleted owners still count: access is about the agency, not about deletion, so a
        // narrative on a deleted incident can still be restored by that incident's agency.
        return ownerType switch
        {
            NarrativeOwnerTypes.Incident => db.Incidents
                .IgnoreQueryFilters([QueryFilterNames.SoftDelete])
                .AnyAsync(x => x.Id == ownerId, cancellationToken),
            NarrativeOwnerTypes.Arrest => db.Arrests
                .IgnoreQueryFilters([QueryFilterNames.SoftDelete])
                .AnyAsync(x => x.Id == ownerId, cancellationToken),
            NarrativeOwnerTypes.Citation => db.Citations
                .IgnoreQueryFilters([QueryFilterNames.SoftDelete])
                .AnyAsync(x => x.Id == ownerId, cancellationToken),
            _ => Task.FromResult(true)
        };
    }

    public static async Task EnsureCanAccessOwnerAsync(
        IApplicationDbContext db,
        string ownerType,
        Guid ownerId,
        CancellationToken cancellationToken)
    {
        if (!await CanAccessOwnerAsync(db, ownerType, ownerId, cancellationToken))
            throw new InvalidOperationException($"{ownerType} record not found.");
    }

    /// <summary>A narrative is accessible when any record it is attached to is accessible.</summary>
    public static async Task<bool> CanAccessNarrativeAsync(
        IApplicationDbContext db,
        Guid narrativeId,
        CancellationToken cancellationToken)
    {
        var owners = await db.NarrativeLinks
            .IgnoreQueryFilters([QueryFilterNames.SoftDelete])
            .Where(l => l.NarrativeId == narrativeId)
            .Select(l => new { l.OwnerType, l.OwnerId })
            .ToListAsync(cancellationToken);

        return await AnyOwnerAccessibleAsync(db, owners.Select(o => (o.OwnerType, o.OwnerId)), cancellationToken);
    }

    public static async Task EnsureCanAccessNarrativeAsync(
        IApplicationDbContext db,
        Guid narrativeId,
        CancellationToken cancellationToken)
    {
        if (!await CanAccessNarrativeAsync(db, narrativeId, cancellationToken))
            throw new InvalidOperationException("Narrative record not found.");
    }

    /// <summary>A mugshot is accessible when any record it is attached to is accessible.</summary>
    public static async Task<bool> CanAccessMugshotAsync(
        IApplicationDbContext db,
        Guid mugshotId,
        CancellationToken cancellationToken)
    {
        var owners = await db.MugshotLinks
            .Where(l => l.MugshotId == mugshotId)
            .Select(l => new { l.OwnerType, l.OwnerId })
            .ToListAsync(cancellationToken);

        return await AnyOwnerAccessibleAsync(db, owners.Select(o => (o.OwnerType, o.OwnerId)), cancellationToken);
    }

    private static async Task<bool> AnyOwnerAccessibleAsync(
        IApplicationDbContext db,
        IEnumerable<(string OwnerType, Guid OwnerId)> owners,
        CancellationToken cancellationToken)
    {
        var any = false;
        foreach (var (ownerType, ownerId) in owners)
        {
            any = true;
            if (await CanAccessOwnerAsync(db, ownerType, ownerId, cancellationToken))
                return true;
        }

        // An unattached record has no owning agency to inherit; the jurisdiction filter still applies.
        return !any;
    }
}
