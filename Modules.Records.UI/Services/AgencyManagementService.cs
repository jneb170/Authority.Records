using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Identity;
using Shared.Infrastructure.Persistence;
using AgencyEntity = Shared.Infrastructure.Identity.Agency;

namespace Modules.Records.UI.Services;

public sealed class AgencyManagementService(
    AuthDbContext authDb,
    UserManager<ApplicationUser> userManager) : IAgencyManagementService
{
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
        await authDb.SaveChangesAsync();
        return agency;
    }

    public async Task<bool> UpdateAsync(Guid id, string name, string code)
    {
        var agency = await authDb.Agencies.FindAsync(id);
        if (agency is null) return false;
        agency.Update(name, code);
        await authDb.SaveChangesAsync();
        return true;
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(Guid id)
    {
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
