using Microsoft.AspNetCore.Identity;
using Shared.Infrastructure.Identity;

namespace Modules.Records.UI.Services;

public sealed record UserDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    bool IsActive,
    Guid JurisdictionId,
    Guid PrimaryAgencyId,
    List<string> Roles,
    List<Guid> AgencyIds);

public interface IUserManagementService
{
    Task<List<UserDto>> GetByJurisdictionAsync(Guid jurisdictionId);
    Task<UserDto?> GetByIdAsync(string userId);
    Task<(IdentityResult Result, string? UserId)> CreateAsync(
        string firstName, string lastName, string email, string password,
        Guid jurisdictionId, Guid primaryAgencyId,
        IEnumerable<string> roles, IEnumerable<Guid> agencyIds);
    Task<IdentityResult> UpdateAsync(
        string userId, string firstName, string lastName, string email, bool isActive,
        Guid primaryAgencyId, IEnumerable<string> roles, IEnumerable<Guid> agencyIds);
    Task<IdentityResult> ResetPasswordAsync(string userId, string newPassword);
    Task<IdentityResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    Task<IdentityResult> DeleteAsync(string userId);
    Task<List<string>> GetAvailableRolesAsync();
    Task<List<Agency>> GetAgenciesForUserAsync(string userId);
}
