using System.Data.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application;
using Modules.Records.Domain.Abstractions;
using Modules.Records.UI.Authorization;
using Modules.Records.UI.Demo;
using Modules.Records.UI.Interop;
using Modules.Records.UI.Middleware;
using Modules.Records.UI.Services;
using Shared.Infrastructure;
using Shared.Infrastructure.Identity;
using Shared.Infrastructure.Maintenance;
using Shared.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// QuestPDF runs under the Community license (free for individuals and organizations with
// annual gross revenue under $1M USD). If this product crosses that threshold, switch to a
// paid license. Must be set once at startup before any PDF is generated, or QuestPDF throws.
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddRazorPages();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Demo-account abuse limits (creation rate + per-save size). Defaults live in
// DemoRateLimitOptions; override in Azure via Demo__RateLimit__* App settings.
builder.Services.AddOptions<Modules.Records.Application.Common.DemoRateLimitOptions>()
    .BindConfiguration(Modules.Records.Application.Common.DemoRateLimitOptions.SectionName);

// Override ITenantProvider with Blazor-aware implementation.
// IHttpContextAccessor.HttpContext is null during Blazor Server SignalR interactions;
// BlazorTenantProvider falls back to AuthenticationStateProvider for claims.
builder.Services.AddScoped<ITenantProvider, BlazorTenantProvider>();
builder.Services.AddScoped<Modules.Records.Application.Abstractions.ICurrentUserContext, BlazorCurrentUserContext>();

builder.Services.AddScoped<IIncidentService, IncidentService>();
builder.Services.AddScoped<IArrestService, ArrestService>();
builder.Services.AddScoped<ICitationService, CitationService>();
builder.Services.AddScoped<IActiveAgencyContext, ActiveAgencyContext>();
builder.Services.AddScoped<IAgencyConfigurationService, AgencyConfigurationService>();
builder.Services.AddScoped<IPicklistService, PicklistService>();
builder.Services.AddScoped<IChargeService, ChargeService>();
builder.Services.AddScoped<INameService, NameService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IMugshotService, MugshotService>();
builder.Services.AddScoped<INarrativeService, NarrativeService>();
builder.Services.AddScoped<Modules.Records.UI.Printing.ICitationTexasPrintModelBuilder, Modules.Records.UI.Printing.CitationTexasPrintModelBuilder>();
builder.Services.AddScoped<IKeyboardShortcutService, KeyboardShortcutService>();
builder.Services.AddScoped<IHotkeyConfigService, HotkeyConfigService>();
builder.Services.AddScoped<IGoogleMapsConfigService, GoogleMapsConfigService>();
builder.Services.AddTransient<IGoogleMapsInterop, GoogleMapsInterop>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IJurisdictionManagementService, JurisdictionManagementService>();
builder.Services.AddScoped<IAgencyManagementService, AgencyManagementService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IRelationshipService, RelationshipService>();

builder.Services.AddHttpContextAccessor();

RecordsAuthorizationPolicies.RegisterPolicies(builder.Services);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseMiddleware<CanonicalHostRedirectMiddleware>();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapStaticAssets();

app.UseAuthentication();
app.UseMiddleware<ApplicationMaintenanceMiddleware>();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorPages();
app.MapRazorComponents<Modules.Records.UI.App>()
    .AddInteractiveServerRenderMode();

var dbProvider = DatabaseProviderResolver.Resolve(app.Configuration);

if (dbProvider == DatabaseProvider.Sqlite)
{
    // SQLite migrations always apply on startup because:
    //  - CI cannot apply them to a file inside Azure App Service.
    //  - Dev local SQLite files are typically per-developer and ephemeral.
    EnsureSqliteDataDirectoriesExist(app.Configuration);

    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<AuthDbContext>().Database.MigrateAsync();

    // One-time, idempotent: renumber any records that were created with random
    // RecordNumbers during the original SQLite cutover (ABS(RANDOM())) back into the
    // short sequential range, then rebuild affected read models. No-op once clean.
    var repairLogger = app.Services.GetRequiredService<ILoggerFactory>()
        .CreateLogger(nameof(SqliteRecordNumberRepair));
    await SqliteRecordNumberRepair.RepairAsync(app.Services, repairLogger);
}
else if (app.Environment.IsDevelopment())
{
    await EnsureAppDbMigrationsAppliedAsync(app.Services);
}

if (app.Environment.IsDevelopment())
{
    // Dev-user seeding is intentionally limited to local development.
    // Production deployments should provision real users explicitly.
    using var scope = app.Services.CreateScope();
    await SeedDevelopmentUsersAsync(scope.ServiceProvider);
}

{
    using var scope = app.Services.CreateScope();
    var maintenanceCoordinator = scope.ServiceProvider.GetRequiredService<ApplicationMaintenanceCoordinator>();
    await maintenanceCoordinator.RunStartupMaintenanceAsync(scope.ServiceProvider);
}

// Seed the public demo account if enabled (default: enabled).
// Idempotent and best-effort — failures are logged but do not block startup.
if (app.Configuration.GetValue("Demo:Enabled", true))
{
    var demoLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("DemoSeeder");
    await DemoSeeder.SeedAsync(app.Services, demoLogger);
}

app.Run();

static void EnsureSqliteDataDirectoriesExist(IConfiguration configuration)
{
    foreach (var key in new[]
             {
                 DatabaseProviderResolver.SqliteAppConnectionStringName,
                 DatabaseProviderResolver.SqliteAuthConnectionStringName
             })
    {
        var raw = configuration.GetConnectionString(key);
        if (string.IsNullOrWhiteSpace(raw))
            continue;

        var expanded = Environment.ExpandEnvironmentVariables(raw);
        var builder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(expanded);
        var dataSource = Path.GetFullPath(builder.DataSource);
        var dir = Path.GetDirectoryName(dataSource);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
    }
}

static async Task EnsureAppDbMigrationsAppliedAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var appDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    IReadOnlyList<string> pendingList;
    try
    {
        pendingList = (await appDb.Database.GetPendingMigrationsAsync()).ToList();
    }
    catch (DbException ex)
    {
        throw new InvalidOperationException(
            "Could not check pending AppDbContext migrations. Ensure the database is reachable " +
            "and run .\\scripts\\update-db.ps1 to apply any outstanding migrations.",
            ex);
    }

    if (pendingList.Count == 0)
        return;

    var migrationSummary = string.Join(", ", pendingList);
    throw new InvalidOperationException(
        "Pending AppDbContext migrations were detected: " +
        $"{migrationSummary}. Run .\\scripts\\update-db.ps1 before starting the app.");
}

static async Task SeedDevelopmentUsersAsync(IServiceProvider services)
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

