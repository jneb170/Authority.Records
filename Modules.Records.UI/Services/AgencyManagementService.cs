using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Identity;
using Shared.Infrastructure.Persistence;
using AgencyEntity = Shared.Infrastructure.Identity.Agency;

namespace Modules.Records.UI.Services;

public sealed class AgencyManagementService(
    AuthDbContext authDb,
    UserManager<ApplicationUser> userManager,
    AuthenticationStateProvider authStateProvider) : IAgencyManagementService
{
    /// <summary>
    /// An agency admin may only change agencies they belong to (issue #118). Super users may
    /// change any. Returns the caller's user ID when it is an agency admin, for membership.
    /// </summary>
    private async Task<(bool IsSuper, string? UserId)> GetCallerAsync()
    {
        var caller = (await authStateProvider.GetAuthenticationStateAsync()).User;
        return (caller.IsInRole(AgencyMembership.SuperRole), caller.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    }

    private async Task<bool> CallerBelongsToAsync(Guid agencyId)
    {
        var (isSuper, userId) = await GetCallerAsync();
        if (isSuper)
            return true;

        return userId is not null
            && (await AgencyMembership.GetAgencyIdsAsync(authDb, userId)).Contains(agencyId);
    }

    public Task<List<AgencyEntity>> GetByJurisdictionAsync(Guid jurisdictionId)
        => authDb.Agencies
            .Where(a => a.JurisdictionId == jurisdictionId)
            .OrderBy(a => a.Name)
            .ToListAsync();

    public Task<AgencyEntity?> GetByIdAsync(Guid id)
        => authDb.Agencies.FirstOrDefaultAsync(a => a.Id == id);

    public async Task<AgencyEntity> CreateAsync(Guid jurisdictionId, string name, string code)
    {
        var agency = AgencyEntity.Create(jurisdictionId, name, code);
        authDb.Agencies.Add(agency);

        // The admin who creates an agency administers it: without membership they could neither
        // assign users to it nor rename it, since admins only manage their own agencies (#118).
        var (isSuper, userId) = await GetCallerAsync();
        if (!isSuper && userId is not null)
            authDb.UserAgencies.Add(ApplicationUserAgency.Create(userId, agency.Id));

        await authDb.SaveChangesAsync();
        return agency;
    }

    public async Task<bool> UpdateAsync(Guid id, string name, string code)
    {
        if (!await CallerBelongsToAsync(id))
            return false;

        var agency = await authDb.Agencies.FindAsync(id);
        if (agency is null) return false;
        agency.Update(name, code);
        await authDb.SaveChangesAsync();
        return true;
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(Guid id)
    {
        if (!await CallerBelongsToAsync(id))
            return (false, UserManagementService.OutsideYourAgencies);

        var hasUsers = await userManager.Users.AnyAsync(u => u.AgencyId == id);
        if (hasUsers)
            return (false, "Cannot delete an agency that has users assigned.");

        var hasUserAgencies = await authDb.UserAgencies.AnyAsync(ua => ua.AgencyId == id);
        if (hasUserAgencies)
            return (false, "Cannot delete an agency that has users assigned.");

        var agency = await authDb.Agencies.FindAsync(id);
        if (agency is null) return (false, "Agency not found.");

        authDb.Agencies.Remove(agency);
        await authDb.SaveChangesAsync();
        return (true, null);
    }
}
