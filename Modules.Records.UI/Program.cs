using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application;
using Modules.Records.Domain.Abstractions;
using Modules.Records.UI.Authorization;
using Modules.Records.UI.Interop;
using Modules.Records.UI.Services;
using Shared.Infrastructure;
using Shared.Infrastructure.Identity;
using Shared.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddRazorPages();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Override ITenantProvider with Blazor-aware implementation.
// IHttpContextAccessor.HttpContext is null during Blazor Server SignalR interactions;
// BlazorTenantProvider falls back to AuthenticationStateProvider for claims.
builder.Services.AddScoped<ITenantProvider, BlazorTenantProvider>();

builder.Services.AddScoped<IIncidentService, IncidentService>();
builder.Services.AddScoped<IArrestService, ArrestService>();
builder.Services.AddScoped<ICitationService, CitationService>();
builder.Services.AddScoped<IAgencyConfigurationService, AgencyConfigurationService>();
builder.Services.AddScoped<IPicklistService, PicklistService>();
builder.Services.AddScoped<INameService, NameService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IMugshotService, MugshotService>();
builder.Services.AddScoped<IKeyboardShortcutService, KeyboardShortcutService>();
builder.Services.AddScoped<IHotkeyConfigService, HotkeyConfigService>();
builder.Services.AddScoped<IGoogleMapsConfigService, GoogleMapsConfigService>();
builder.Services.AddTransient<IGoogleMapsInterop, GoogleMapsInterop>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IJurisdictionManagementService, JurisdictionManagementService>();
builder.Services.AddScoped<IAgencyManagementService, AgencyManagementService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();

builder.Services.AddHttpContextAccessor();

RecordsAuthorizationPolicies.RegisterPolicies(builder.Services);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapStaticAssets();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorPages();
app.MapRazorComponents<Modules.Records.UI.App>()
    .AddInteractiveServerRenderMode();

if (app.Environment.IsDevelopment())
{
    await EnsureAppDbMigrationsAppliedAsync(app.Services);
}

// Seed initial users and roles on first run (any environment).
// SeedDevUserAsync is idempotent — it only creates users if they don't exist.
{
    using var scope = app.Services.CreateScope();
    await SeedDevUserAsync(scope.ServiceProvider);
}

app.Run();

static async Task EnsureAppDbMigrationsAppliedAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var appDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var pendingMigrations = await appDb.Database.GetPendingMigrationsAsync();
    var pendingList = pendingMigrations.ToList();

    if (pendingList.Count == 0)
        return;

    var migrationSummary = string.Join(", ", pendingList);
    throw new InvalidOperationException(
        "Pending AppDbContext migrations were detected: " +
        $"{migrationSummary}. Run .\\scripts\\update-db.ps1 before starting the app.");
}

static async Task SeedDevUserAsync(IServiceProvider services)
{
    var authDb = services.GetRequiredService<AuthDbContext>();
    await authDb.Database.MigrateAsync();

    var roleMgr = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = services.GetRequiredService<UserManager<ApplicationUser>>();

    // Ensure roles exist
    foreach (var role in new[] { "Super", "Admin", "Supervisor", "Officer", "Dispatcher" })
    {
        if (!await roleMgr.RoleExistsAsync(role))
            await roleMgr.CreateAsync(new IdentityRole(role));
    }

    // Seed Super user (no jurisdiction/agency — manages system setup only)
    const string superEmail = "super@authority.local";
    var superUser = await userMgr.FindByEmailAsync(superEmail);
    if (superUser is null)
    {
        superUser = new ApplicationUser
        {
            UserName = superEmail,
            Email = superEmail,
            EmailConfirmed = true,
            FirstName = "System",
            LastName = "Super",
            IsActive = true,
        };
        await userMgr.CreateAsync(superUser, "Super@1234");
        superUser = await userMgr.FindByEmailAsync(superEmail);
    }
    if (superUser is not null && !await userMgr.IsInRoleAsync(superUser, "Super"))
        await userMgr.AddToRoleAsync(superUser, "Super");

    const string email = "admin@authority.local";
    var existing = await userMgr.FindByEmailAsync(email);

    if (existing is null)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = "Dev",
            LastName = "Admin",
            IsActive = true,
            JurisdictionId = new Guid("11111111-1111-1111-1111-111111111111"),
            AgencyId = new Guid("22222222-2222-2222-2222-222222222222"),
        };

        await userMgr.CreateAsync(user, "Test@1234");
        existing = await userMgr.FindByEmailAsync(email);
    }

    // Ensure dev user is in Admin role
    if (existing is not null && !await userMgr.IsInRoleAsync(existing, "Admin"))
        await userMgr.AddToRoleAsync(existing, "Admin");
}

