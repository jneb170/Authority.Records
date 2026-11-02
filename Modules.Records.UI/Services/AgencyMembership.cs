using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Persistence;

namespace Modules.Records.UI.Services;

/// <summary>
/// The agencies a user belongs to: their primary agency plus any additional assignments.
/// Admin pages use this to keep an agency admin inside their own agencies (issue #118):
/// an admin can only see and manage users who share one of their agencies, and can only grant
/// agencies they belong to. Otherwise editing any user's agency list, their own included,
/// would grant access to another agency's incidents, arrests and citations.
/// </summary>
internal static class AgencyMembership
{
    public const string SuperRole = "Super";

    public static async Task<HashSet<Guid>> GetAgencyIdsAsync(AuthDbContext db, string userId)
    {
        var primary = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.AgencyId)
            .FirstOrDefaultAsync();

        var assigned = await db.UserAgencies
            .AsNoTracking()
            .Where(ua => ua.UserId == userId)
            .Select(ua => ua.AgencyId)
            .ToListAsync();

        var ids = assigned.ToHashSet();
        if (primary != Guid.Empty)
            ids.Add(primary);
        return ids;
    }
}
