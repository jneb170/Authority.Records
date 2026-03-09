using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Identity;
using Shared.Infrastructure.Persistence;

namespace Modules.Records.UI.Services;

public sealed class UserManagementService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    AuthDbContext authDb) : IUserManagementService
{
    public async Task<List<UserDto>> GetByJurisdictionAsync(Guid jurisdictionId)
    {
        var users = await userManager.Users
            .Where(u => u.JurisdictionId == jurisdictionId)
            .OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
            .ToListAsync();

        var result = new List<UserDto>();
        foreach (var u in users)
        {
            var roles = (await userManager.GetRolesAsync(u)).ToList();
            var agencyIds = await authDb.UserAgencies
                .Where(ua => ua.UserId == u.Id)
                .Select(ua => ua.AgencyId)
                .ToListAsync();
            result.Add(ToDto(u, roles, agencyIds));
        }
        return result;
    }

    public async Task<UserDto?> GetByIdAsync(string userId)
    {
        var u = await userManager.FindByIdAsync(userId);
        if (u is null) return null;
        var roles = (await userManager.GetRolesAsync(u)).ToList();
        var agencyIds = await authDb.UserAgencies
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
        user.IsActive = isActive;
        user.AgencyId = primaryAgencyId;

        var result = await userManager.UpdateAsync(user);
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
        return await roleManager.Roles
            .Where(r => !excluded.Contains(r.Name))
            .Select(r => r.Name!)
            .OrderBy(r => r)
            .ToListAsync();
    }

    public async Task<List<Agency>> GetAgenciesForUserAsync(string userId)
    {
        var agencyIds = await authDb.UserAgencies
            .Where(ua => ua.UserId == userId)
            .Select(ua => ua.AgencyId)
            .ToListAsync();

        return await authDb.Agencies
            .Where(a => agencyIds.Contains(a.Id))
            .OrderBy(a => a.Name)
            .ToListAsync();
    }

    private static UserDto ToDto(ApplicationUser u, List<string> roles, List<Guid> agencyIds)
        => new(u.Id, u.Email ?? string.Empty, u.FirstName, u.LastName, u.FullName,
               u.IsActive, u.JurisdictionId, u.AgencyId, roles, agencyIds);
}
