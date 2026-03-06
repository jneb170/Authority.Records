using Microsoft.AspNetCore.Identity;

namespace Shared.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser
{
    public Guid JurisdictionId { get; set; }
    public Guid AgencyId { get; set; }
}
