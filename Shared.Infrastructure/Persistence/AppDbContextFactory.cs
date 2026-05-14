using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.DomainEvents;

namespace Shared.Infrastructure.Persistence
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // 1️ Build configuration to get connection string
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()   // allows CI/CD to override via env vars
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            var provider = DatabaseProviderResolver.Resolve(configuration);
            var connectionString = DatabaseProviderResolver.GetConnectionString(
                configuration, provider, isAuth: false);
            var migrationsAssembly = typeof(AppDbContext).Assembly.FullName;

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

            // 2️ Provide dummy implementations for tenantProvider & domainEventDispatcher
            return new AppDbContext(
                optionsBuilder.Options,
                new DummyTenantProvider(),
                new DummyDomainEventDispatcher());
        }

        // --- Dummy tenant provider ---
        private class DummyTenantProvider : ITenantProvider
        {
            public Guid JurisdictionId => throw new NotImplementedException();

            public Guid? AgencyId => throw new NotImplementedException();

            public Guid? UserId => throw new NotImplementedException();

            public Guid GetAgencyId()
            {
                throw new NotImplementedException();
            }

            public Guid GetJurisdictionId() => Guid.Empty;

            public Guid GetUserId()
            {
                throw new NotImplementedException();
            }

            public void SetJurisdictionId(Guid jurisdictionId)
            {
                throw new NotImplementedException();
            }
        }

        // --- Dummy domain event dispatcher ---
        private class DummyDomainEventDispatcher : IDomainEventDispatcher
        {
            public Task DispatchAsync(
                IDomainEvent domainEvent,
                CancellationToken cancellationToken = default)
                => Task.CompletedTask;

            public Task DispatchAsync(
                IEnumerable<IDomainEvent> domainEvents, 
                CancellationToken cancellationToken = default)
                => Task.CompletedTask;
        }
    }
}
