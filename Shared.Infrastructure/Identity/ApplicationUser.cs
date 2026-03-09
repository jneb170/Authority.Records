using Microsoft.AspNetCore.Identity;

namespace Shared.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser
{
    public Guid JurisdictionId { get; set; }
    public Guid AgencyId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public string FullName => $"{FirstName} {LastName}".Trim();
}
