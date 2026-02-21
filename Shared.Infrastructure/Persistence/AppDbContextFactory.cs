using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
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
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"));

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
