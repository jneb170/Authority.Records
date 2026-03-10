using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Shared.Infrastructure.Persistence;

public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()   // allows CI/CD to override via env vars
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? configuration["DefaultConnection"];

        optionsBuilder.UseSqlServer(
            connectionString,
            sql => sql.MigrationsAssembly(typeof(AuthDbContext).Assembly.FullName));

        return new AuthDbContext(optionsBuilder.Options);
    }
}
