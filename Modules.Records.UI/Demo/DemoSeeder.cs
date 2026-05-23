using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;
using Modules.Records.Domain.Factories;
using Modules.Records.Domain.ValueObjects;
using Shared.Infrastructure.Identity;
using Shared.Infrastructure.Persistence;

namespace Modules.Records.UI.Demo;

/// <summary>
/// Idempotent startup seeder for the public demo account.
/// Creates the demo jurisdiction, agency, a credentialed admin user, the
/// passwordless demo user, and a small set of sample incidents/arrests/
/// citations so the "Try the demo" landing experience has something to look
/// at. The demo user is seeded with NO role — an administrator assigns its
/// permissions in /admin/users, so the demo agency behaves like any other.
/// Safe to run on every boot — every step checks for existence before writing.
/// </summary>
public static class DemoSeeder
{
    public static async Task SeedAsync(IServiceProvider services, ILogger logger)
    {
        try
        {
            await using var scope = services.CreateAsyncScope();
            var sp = scope.ServiceProvider;

            var authDb = sp.GetRequiredService<AuthDbContext>();
            var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = sp.GetRequiredService<UserManager<ApplicationUser>>();

            await EnsureRoleAsync(roleMgr, "Admin");

            // One-time cleanup: the demo user used to carry a read-only "Demo"
            // role enforced by a write guard. That concept is gone — the demo
            // user is now an ordinary account whose permissions an admin sets in
            // /admin/users. Strip any lingering "Demo" assignments and drop the
            // role so it no longer appears as assignable. Idempotent: no-ops once
            // the role is gone.
            await RemoveLegacyDemoRoleAsync(roleMgr, userMgr);

            var jurisdiction = await EnsureJurisdictionAsync(authDb);
            var agency = await EnsureAgencyAsync(authDb, jurisdiction.Id);

            // Seeded with no role on purpose — an admin assigns permissions later.
            var demoUser = await EnsureUserAsync(
                userMgr,
                email: DemoUserDefaults.Email,
                password: DemoUserDefaults.Password,
                firstName: DemoUserDefaults.FirstName,
                lastName: DemoUserDefaults.LastName,
                role: null,
                jurisdictionId: jurisdiction.Id,
                agencyId: agency.Id);
            await EnsureUserAgencyAsync(authDb, demoUser.Id, agency.Id);

            var adminUser = await EnsureUserAsync(
                userMgr,
                email: DemoUserDefaults.AdminEmail,
                password: DemoUserDefaults.AdminPassword,
                firstName: DemoUserDefaults.AdminFirstName,
                lastName: DemoUserDefaults.AdminLastName,
                role: "Admin",
                jurisdictionId: jurisdiction.Id,
                agencyId: agency.Id);
            await EnsureUserAgencyAsync(authDb, adminUser.Id, agency.Id);

            // Sample data is best-effort and isolated from identity setup —
            // failures here must not block sign-in.
            try
            {
                await EnsureSampleRecordsAsync(sp, jurisdiction.Id, agency.Id);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Demo sample record seeding failed; demo user can still sign in.");
            }

            logger.LogInformation(
                "Demo account ready: user={Email}, admin={AdminEmail}, agency={Agency}, jurisdiction={Jurisdiction}.",
                DemoUserDefaults.Email, DemoUserDefaults.AdminEmail, agency.Name, jurisdiction.Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Demo seeding failed; the Try-the-demo button will not work until this is resolved.");
        }
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleMgr, string roleName)
    {
        if (!await roleMgr.RoleExistsAsync(roleName))
            await roleMgr.CreateAsync(new IdentityRole(roleName));
    }

    /// <summary>
    /// Removes the obsolete read-only "Demo" role: unassigns it from every user
    /// (the public demo account in particular) and deletes the role itself.
    /// Safe to call repeatedly — once the role is gone this is a no-op.
    /// </summary>
    private static async Task RemoveLegacyDemoRoleAsync(
        RoleManager<IdentityRole> roleMgr, UserManager<ApplicationUser> userMgr)
    {
        const string legacyDemoRole = "Demo";

        var role = await roleMgr.FindByNameAsync(legacyDemoRole);
        if (role is null) return;

        foreach (var user in await userMgr.GetUsersInRoleAsync(legacyDemoRole))
            await userMgr.RemoveFromRoleAsync(user, legacyDemoRole);

        await roleMgr.DeleteAsync(role);
    }

    private static async Task<Jurisdiction> EnsureJurisdictionAsync(AuthDbContext authDb)
    {
        var existing = await authDb.Jurisdictions
            .FirstOrDefaultAsync(j => j.Code == DemoUserDefaults.JurisdictionCode);
        if (existing is not null) return existing;

        var jurisdiction = Jurisdiction.Create(
            DemoUserDefaults.JurisdictionName,
            DemoUserDefaults.JurisdictionState,
            DemoUserDefaults.JurisdictionCode);
        authDb.Jurisdictions.Add(jurisdiction);
        await authDb.SaveChangesAsync();
        return jurisdiction;
    }

    private static async Task<Agency> EnsureAgencyAsync(AuthDbContext authDb, Guid jurisdictionId)
    {
        var existing = await authDb.Agencies
            .FirstOrDefaultAsync(a => a.JurisdictionId == jurisdictionId && a.Code == DemoUserDefaults.AgencyCode);
        if (existing is not null) return existing;

        var agency = Agency.Create(jurisdictionId, DemoUserDefaults.AgencyName, DemoUserDefaults.AgencyCode);
        authDb.Agencies.Add(agency);
        await authDb.SaveChangesAsync();
        return agency;
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userMgr,
        string email,
        string password,
        string firstName,
        string lastName,
        string? role,
        Guid jurisdictionId,
        Guid agencyId)
    {
        var existing = await userMgr.FindByEmailAsync(email);
        if (existing is not null)
        {
            // Repair tenant pointers in case agency/jurisdiction was re-seeded.
            // Password is intentionally not reset — admins may have changed it.
            if (existing.JurisdictionId != jurisdictionId || existing.AgencyId != agencyId)
            {
                existing.JurisdictionId = jurisdictionId;
                existing.AgencyId = agencyId;
                await userMgr.UpdateAsync(existing);
            }
            // Roles are only ensured, never removed — an admin may have changed
            // them in /admin/users. A null role means "leave permissions alone".
            if (role is not null && !await userMgr.IsInRoleAsync(existing, role))
                await userMgr.AddToRoleAsync(existing, role);
            return existing;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
            IsActive = true,
            JurisdictionId = jurisdictionId,
            AgencyId = agencyId,
        };

        var create = await userMgr.CreateAsync(user, password);
        if (!create.Succeeded)
            throw new InvalidOperationException(
                $"Could not create user '{email}': " + string.Join("; ", create.Errors.Select(e => e.Description)));

        if (role is not null)
            await userMgr.AddToRoleAsync(user, role);
        return (await userMgr.FindByEmailAsync(email))!;
    }

    private static async Task EnsureUserAgencyAsync(AuthDbContext authDb, string userId, Guid agencyId)
    {
        var exists = await authDb.UserAgencies.AnyAsync(ua => ua.UserId == userId && ua.AgencyId == agencyId);
        if (exists) return;

        authDb.UserAgencies.Add(ApplicationUserAgency.Create(userId, agencyId));
        await authDb.SaveChangesAsync();
    }

    private static async Task EnsureSampleRecordsAsync(IServiceProvider sp, Guid jurisdictionId, Guid agencyId)
    {
        var appDb = sp.GetRequiredService<AppDbContext>();
        var tenantProvider = sp.GetRequiredService<ITenantProvider>();

        // Domain-event projection handlers resolve their own AppDbContext,
        // which uses this scoped tenant provider for global query filters.
        tenantProvider.SetJurisdictionId(jurisdictionId);

        // Already seeded? Skip the lot.
        var alreadySeeded = await appDb.Incidents
            .AnyAsync(i => i.AgencyId == agencyId);
        if (alreadySeeded) return;

        var incidentFactory = sp.GetRequiredService<IncidentFactory>();
        var arrestFactory = sp.GetRequiredService<ArrestFactory>();

        // ---- Incidents ----
        var incidents = new[]
        {
            BuildIncident(incidentFactory, jurisdictionId, agencyId, "DEMO-2026-001", "ALARM-001",
                "Burglar alarm activation at 4521 Maple Ave. Patrol responded; building secure on arrival."),
            BuildIncident(incidentFactory, jurisdictionId, agencyId, "DEMO-2026-002", "TRAFFIC-001",
                "Two-vehicle collision at the intersection of 5th and Main. No injuries reported."),
            BuildIncident(incidentFactory, jurisdictionId, agencyId, "DEMO-2026-003", "TRESPASS-001",
                "Trespass complaint at the public library. Subject identified and trespass-warned."),
            BuildIncident(incidentFactory, jurisdictionId, agencyId, "DEMO-2026-004", "WELFARE-001",
                "Welfare check on elderly resident at 22 Oak Street. Subject contacted and well."),
        };
        appDb.Incidents.AddRange(incidents);

        // ---- Arrests ----
        var arrests = new[]
        {
            arrestFactory.Create(jurisdictionId, agencyId, nameId: null,
                arrestedAt: DateTime.UtcNow.AddDays(-3),
                arrestNum: "DEMO-AR-2026-001",
                primaryIncidentId: null),
            arrestFactory.Create(jurisdictionId, agencyId, nameId: null,
                arrestedAt: DateTime.UtcNow.AddDays(-1),
                arrestNum: "DEMO-AR-2026-002",
                primaryIncidentId: null),
        };
        appDb.Arrests.AddRange(arrests);

        // ---- Citations ----
        var citations = new[]
        {
            new Citation(jurisdictionId, agencyId,
                description: "Speeding 45 in a 35 zone — issued at Main & 3rd.",
                issueDate: DateTime.UtcNow.AddDays(-2),
                citationNum: "DEMO-CT-2026-001"),
            new Citation(jurisdictionId, agencyId,
                description: "Failure to yield at posted stop sign — Oak & Center.",
                issueDate: DateTime.UtcNow.AddDays(-1),
                citationNum: "DEMO-CT-2026-002"),
        };
        appDb.Citations.AddRange(citations);

        await appDb.SaveChangesAsync();
    }

    private static Incident BuildIncident(
        IncidentFactory factory,
        Guid jurisdictionId,
        Guid agencyId,
        string incidentNum,
        string localNum,
        string description)
    {
        return factory.Create(new CreateIncidentRequest
        {
            JurisdictionId = jurisdictionId,
            AgencyId = agencyId,
            Details = new IncidentDetails
            {
                IncidentNum = incidentNum,
                LocalNum = localNum,
                Description = description,
                CFSNum = string.Empty,
            },
        });
    }
}
