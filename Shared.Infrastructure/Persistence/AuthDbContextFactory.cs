using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;

namespace Shared.Infrastructure.Persistence;

public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()   // allows CI/CD to override via env vars
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();

        var provider = DatabaseProviderResolver.Resolve(configuration);
        var connectionString = DatabaseProviderResolver.GetConnectionString(
            configuration, provider, isAuth: true);
        var migrationsAssembly = typeof(AuthDbContext).Assembly.FullName;

        if (provider == DatabaseProvider.SqlServer)
        {
            optionsBuilder.UseSqlServer(connectionString,
                sql => sql.MigrationsAssembly(migrationsAssembly));
            optionsBuilder.ReplaceService<IMigrationsAssembly, SqlServerMigrationsAssembly>();
        }
        else
        {
            optionsBuilder.UseSqlite(connectionString,
                sqlite => sqlite.MigrationsAssembly(migrationsAssembly));
            optionsBuilder.ReplaceService<IMigrationsAssembly, SqliteMigrationsAssembly>();
        }

        return new AuthDbContext(optionsBuilder.Options);
    }
}
