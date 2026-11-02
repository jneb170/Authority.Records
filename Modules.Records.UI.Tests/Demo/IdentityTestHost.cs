using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure;
using Shared.Infrastructure.Identity;
using Shared.Infrastructure.Persistence;

namespace Modules.Records.UI.Tests.Demo;

/// <summary>
/// The production Identity wiring (<see cref="DependencyInjection.AddRecordsIdentity"/>) over an in-memory
/// SQLite <see cref="AuthDbContext"/>, for tests of the demo account flows.
/// </summary>
internal sealed class IdentityTestHost : IDisposable
{
    public const string Password = "Passw0rd!";

    private readonly SqliteConnection _connection;

    public ServiceProvider Provider { get; }
    public IConfiguration Configuration { get; }

    public IdentityTestHost(Dictionary<string, string?>? settings = null, Action<IServiceCollection>? configure = null)
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        Configuration = new ConfigurationBuilder().AddInMemoryCollection(settings ?? new()).Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpContextAccessor();
        services.AddSingleton(Configuration);
        services.AddDbContext<AuthDbContext>(o => o.UseSqlite(_connection));
        services.AddRecordsIdentity();
        configure?.Invoke(services);

        Provider = services.BuildServiceProvider();

        using var scope = Provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<AuthDbContext>().Database.EnsureCreated();
    }

    /// <summary>A scope with an HttpContext, as SignInManager requires.</summary>
    public IServiceScope CreateRequestScope()
    {
        var scope = Provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext =
            new DefaultHttpContext { RequestServices = scope.ServiceProvider };
        return scope;
    }

    public async Task<ApplicationUser> CreateUserAsync(string email, string? role = null, Guid? jurisdictionId = null, Guid? agencyId = null)
    {
        using var scope = Provider.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = "Test",
            LastName = "User",
            IsActive = true,
            JurisdictionId = jurisdictionId ?? Guid.NewGuid(),
            AgencyId = agencyId ?? Guid.NewGuid(),
        };
        var result = await users.CreateAsync(user, Password);
        Assert.True(result.Succeeded, string.Join("; ", result.Errors.Select(e => e.Description)));

        if (role is not null)
        {
            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            if (!await roles.RoleExistsAsync(role))
                await roles.CreateAsync(new IdentityRole(role));
            await users.AddToRoleAsync(user, role);
        }

        return user;
    }

    public void Dispose()
    {
        Provider.Dispose();
        _connection.Dispose();
    }
}
