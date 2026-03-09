using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Shared.Infrastructure.Identity;

public sealed class RecordsUserClaimsPrincipalFactory
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    public RecordsUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor) { }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        // Super users manage jurisdictions/admins only — no tenant context needed
        var roles = await UserManager.GetRolesAsync(user);
        if (roles.Contains("Super"))
            return identity;

        identity.AddClaim(new Claim("jurisdiction", user.JurisdictionId.ToString()));
        identity.AddClaim(new Claim("agency", user.AgencyId.ToString()));

        return identity;
    }
}
