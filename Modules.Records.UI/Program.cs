using Microsoft.AspNetCore.Identity;
using Modules.Records.Application;
using Modules.Records.Domain.Abstractions;
using Modules.Records.UI.Authorization;
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

// Seed dev test user
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    await SeedDevUserAsync(scope.ServiceProvider);
}

app.Run();

static async Task SeedDevUserAsync(IServiceProvider services)
{
    var authDb = services.GetRequiredService<AuthDbContext>();
    await authDb.Database.EnsureCreatedAsync();

    var userMgr = services.GetRequiredService<UserManager<ApplicationUser>>();

    const string email = "admin@authority.local";
    if (await userMgr.FindByEmailAsync(email) is null)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            JurisdictionId = new Guid("11111111-1111-1111-1111-111111111111"),
            AgencyId = new Guid("22222222-2222-2222-2222-222222222222"),
        };

        await userMgr.CreateAsync(user, "Test@1234");
    }
}

