using Infrastructure.IntegrationTests.Outbox.TenantIsolation;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.DomainEvents;
using Shared.Infrastructure.DomainEvents;
using Shared.Infrastructure.Outbox;
using Shared.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.IntegrationTests.Common
{
    public abstract class IntegrationTestBase : IDisposable
    {
        private readonly SqliteConnection _connection;

        protected readonly ServiceProvider ServiceProvider;

        protected IntegrationTestBase()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            var services = new ServiceCollection();

            services.AddLogging(); //required for MediatR

            services.AddDbContext<AppDbContext, TestAppDbContext>(options =>
                options.UseSqlite(_connection));

            services.AddScoped<ITenantProvider>(sp => new TestTenantProvider(Guid.NewGuid()));
            //services.AddScoped<ITenantProvider, TestTenantProvider>();
            services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();

            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(IntegrationTestBase).Assembly));

            services.AddScoped<OutboxProcessor>();

            //adding TestTenantIsolationDomainEvent to DomainEventTypeRegistry
            services.AddSingleton(sp =>
                new DomainEventTypeRegistry(
                    typeof(IDomainEvent).Assembly,
                    typeof(TestTenantIsolationDomainEvent).Assembly
                ));

            ServiceProvider = services.BuildServiceProvider();

            using var scope = ServiceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();

        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }

}
