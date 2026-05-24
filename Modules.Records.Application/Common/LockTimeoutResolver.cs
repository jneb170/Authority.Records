using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Common;

namespace Modules.Records.Application.Common;

/// <summary>
/// Reads an agency's configured record-lock timeout from <see cref="IApplicationDbContext.AgencyConfigurations"/>,
/// falling back to <see cref="ConfigurationKeys.DefaultLockTimeoutSeconds"/> when unset.
/// Used by the lock-acquisition handlers so the timeout passed to
/// <c>AcquireLock</c> reflects the record's owning agency.
/// </summary>
public static class LockTimeoutResolver
{
    public static async Task<TimeSpan> ResolveAsync(
        IApplicationDbContext db,
        Guid agencyId,
        CancellationToken cancellationToken)
    {
        var value = await db.AgencyConfigurations
            .AsNoTracking()
            .Where(c => c.AgencyId == agencyId && c.Key == ConfigurationKeys.LockTimeoutSeconds)
            .Select(c => c.Value)
            .FirstOrDefaultAsync(cancellationToken);

        return LockTimeout.FromConfigValue(value);
    }

    /// <summary>
    /// Resolves a lock timeout from an arbitrary config key with an explicit default (seconds).
    /// Used for Narratives, which deliberately use a much longer timeout
    /// (<see cref="ConfigurationKeys.NarrativeLockTimeoutSeconds"/>) than ordinary records.
    /// </summary>
    public static async Task<TimeSpan> ResolveAsync(
        IApplicationDbContext db,
        Guid agencyId,
        string configKey,
        int defaultSeconds,
        CancellationToken cancellationToken)
    {
        var value = await db.AgencyConfigurations
            .AsNoTracking()
            .Where(c => c.AgencyId == agencyId && c.Key == configKey)
            .Select(c => c.Value)
            .FirstOrDefaultAsync(cancellationToken);

        return int.TryParse(value, out var seconds) && seconds > 0
            ? TimeSpan.FromSeconds(seconds)
            : TimeSpan.FromSeconds(defaultSeconds);
    }
}
