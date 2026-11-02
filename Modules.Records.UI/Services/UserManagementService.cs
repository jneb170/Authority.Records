using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Common;
using Shared.Infrastructure.Identity;
using Shared.Infrastructure.Persistence;

namespace Modules.Records.UI.Services;

public sealed class UserManagementService(
    UserManager<ApplicationUser> userManager,
    AuthDbContext authDb,
    IServiceScopeFactory serviceScopeFactory,
    IConfiguration configuration) : IUserManagementService
{
    private readonly string _demoEmail = DemoUserMatching.ResolveDemoEmail(configuration[DemoUserMatching.LoginEmailConfigKey]);

    /// <summary>Shown when someone tries to change a demo account's password (issue #91).</summary>
    public const string DemoPasswordChangeRefused =
        "Demo accounts can't change their password. Sign in with your own account to manage a password.";

    public async Task<bool> IsDemoAccountAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user is not null && await IsDemoAccountAsync(user);
    }

    private async Task<bool> IsDemoAccountAsync(ApplicationUser user)
        => DemoUserMatching.IsDemoEmail(user.Email, _demoEmail)
           || DemoUserMatching.IsDemoEmail(user.UserName, _demoEmail)
           || (await userManager.GetClaimsAsync(user)).Any(c => c.Type == DemoUserMatching.DemoVisitorClaimType);

    public async Task<List<UserDto>> GetAllAsync()
    {
        using var scope = serviceScopeFactory.CreateScope();
        var readDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var users = await readDb.Users
            .AsNoTracking()
            .OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
            .ToListAsync();

        return await BuildUserDtosAsync(readDb, users);
    }

    public async Task<List<UserDto>> GetByJurisdictionAsync(Guid jurisdictionId)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var readDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var users = await readDb.Users
            .AsNoTracking()
            .Where(u => u.JurisdictionId == jurisdictionId)
            .OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
            .ToListAsync();

        return await BuildUserDtosAsync(readDb, users);
    }

    public async Task<UserDto?> GetByIdAsync(string userId)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var readDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var u = await readDb.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == userId);

        if (u is null) return null;

        var roles = await readDb.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(
                readDb.Roles,
                userRole => userRole.RoleId,
                role => role.Id,
                (userRole, role) => role.Name ?? string.Empty)
            .Where(roleName => roleName.Length > 0)
            .ToListAsync();

        var agencyIds = await readDb.UserAgencies
            .Where(ua => ua.UserId == u.Id)
            .Select(ua => ua.AgencyId)
            .ToListAsync();

        return ToDto(u, roles, agencyIds);
    }

    public async Task<(IdentityResult Result, string? UserId)> CreateAsync(
        string firstName, string lastName, string email, string password,
        Guid jurisdictionId, Guid primaryAgencyId,
        IEnumerable<string> roles, IEnumerable<Guid> agencyIds)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
            IsActive = true,
            JurisdictionId = jurisdictionId,
            AgencyId = primaryAgencyId,
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded) return (result, null);

        var roleList = roles.ToList();
        if (roleList.Count > 0)
            await userManager.AddToRolesAsync(user, roleList);

        var agencyList = agencyIds.ToList();
        if (agencyList.Count > 0)
        {
            authDb.UserAgencies.AddRange(
                agencyList.Select(aId => ApplicationUserAgency.Create(user.Id, aId)));
            await authDb.SaveChangesAsync();
        }

        return (result, user.Id);
    }

    public async Task<IdentityResult> UpdateAsync(
        string userId, string firstName, string lastName, string email, bool isActive,
        Guid primaryAgencyId, IEnumerable<string> roles, IEnumerable<Guid> agencyIds)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return IdentityResult.Failed(new IdentityError { Description = "User not found." });

        user.FirstName = firstName;
        user.LastName = lastName;
        user.Email = email;
        user.UserName = email;
        user.NormalizedEmail = email.ToUpperInvariant();
        user.NormalizedUserName = email.ToUpperInvariant();
        var deactivating = user.IsActive && !isActive;
        user.IsActive = isActive;
        user.AgencyId = primaryAgencyId;

        // Deactivation rotates the security stamp (saving the other changes in the same
        // write) so the user's existing auth cookies fail validation on their next check.
        var result = deactivating
            ? await userManager.UpdateSecurityStampAsync(user)
            : await userManager.UpdateAsync(user);
        if (!result.Succeeded) return result;

        // Sync roles
        var currentRoles = await userManager.GetRolesAsync(user);
        var desiredRoles = roles.ToList();
        var toAdd = desiredRoles.Except(currentRoles).ToList();
        var toRemove = currentRoles.Except(desiredRoles).ToList();
        if (toRemove.Count > 0) await userManager.RemoveFromRolesAsync(user, toRemove);
        if (toAdd.Count > 0) await userManager.AddToRolesAsync(user, toAdd);

        // Sync agency assignments
        var existing = await authDb.UserAgencies.Where(ua => ua.UserId == userId).ToListAsync();
        authDb.UserAgencies.RemoveRange(existing);
        var desiredAgencies = agencyIds.ToList();
        if (desiredAgencies.Count > 0)
            authDb.UserAgencies.AddRange(desiredAgencies.Select(aId => ApplicationUserAgency.Create(userId, aId)));
        await authDb.SaveChangesAsync();

        return result;
    }

    public async Task<IdentityResult> ResetPasswordAsync(string userId, string newPassword)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return IdentityResult.Failed(new IdentityError { Description = "User not found." });

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        return await userManager.ResetPasswordAsync(user, token, newPassword);
    }

    public async Task<IdentityResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return IdentityResult.Failed(new IdentityError { Description = "User not found." });

        // The shared demo account's password is public, and changing it rotates the security stamp,
        // which signs every other visitor out within a minute (issue #91). Enforced here, not only in the UI.
        if (await IsDemoAccountAsync(user))
            return IdentityResult.Failed(new IdentityError { Code = "DemoAccount", Description = DemoPasswordChangeRefused });

        return await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
    }

    public async Task<IdentityResult> DeleteAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return IdentityResult.Failed(new IdentityError { Description = "User not found." });

        var agencies = await authDb.UserAgencies.Where(ua => ua.UserId == userId).ToListAsync();
        authDb.UserAgencies.RemoveRange(agencies);
        await authDb.SaveChangesAsync();

        return await userManager.DeleteAsync(user);
    }

    public async Task<List<string>> GetAvailableRolesAsync()
    {
        var excluded = new[] { "Super" };
        using var scope = serviceScopeFactory.CreateScope();
        var readDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        return await readDb.Roles
            .Where(r => !excluded.Contains(r.Name))
            .Select(r => r.Name!)
            .OrderBy(r => r)
            .ToListAsync();
    }

    public async Task<List<Agency>> GetAgenciesForUserAsync(string userId)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var readDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var user = await readDb.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == userId);

        if (user is null)
            return [];

        var agencyIds = await readDb.UserAgencies
            .Where(ua => ua.UserId == userId)
            .Select(ua => ua.AgencyId)
            .ToListAsync();

        if (user.AgencyId != Guid.Empty && !agencyIds.Contains(user.AgencyId))
            agencyIds.Add(user.AgencyId);

        return await readDb.Agencies
            .Where(a => a.JurisdictionId == user.JurisdictionId && agencyIds.Contains(a.Id))
            .OrderBy(a => a.Name)
            .ToListAsync();
    }

    private static UserDto ToDto(ApplicationUser u, List<string> roles, List<Guid> agencyIds)
        => new(u.Id, u.Email ?? string.Empty, u.FirstName, u.LastName, u.FullName,
               u.IsActive, u.JurisdictionId, u.AgencyId, roles, agencyIds);

    private static async Task<List<UserDto>> BuildUserDtosAsync(AuthDbContext readDb, List<ApplicationUser> users)
    {
        if (users.Count == 0) return [];

        var userIds = users.Select(u => u.Id).ToList();

        var roleNames = await readDb.Roles.ToDictionaryAsync(r => r.Id, r => r.Name ?? string.Empty);
        var userRoleEntries = await readDb.UserRoles
            .Where(ur => userIds.Contains(ur.UserId))
            .ToListAsync();
        var rolesByUserId = userRoleEntries
            .GroupBy(ur => ur.UserId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(ur => roleNames.GetValueOrDefault(ur.RoleId, string.Empty))
                      .Where(n => n.Length > 0)
                      .ToList());

        var agencyAssignments = await readDb.UserAgencies
            .Where(ua => userIds.Contains(ua.UserId))
            .ToListAsync();
        var agenciesByUserId = agencyAssignments
            .GroupBy(ua => ua.UserId)
            .ToDictionary(g => g.Key, g => g.Select(ua => ua.AgencyId).ToList());

        return users.Select(u => ToDto(
            u,
            rolesByUserId.GetValueOrDefault(u.Id, []),
            agenciesByUserId.GetValueOrDefault(u.Id, []))).ToList();
    }
}
