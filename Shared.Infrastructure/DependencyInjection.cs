using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.DomainEvents;
using Shared.Infrastructure.Audit;
using Shared.Infrastructure.DomainEvents;
using Shared.Infrastructure.Identity;
using Shared.Infrastructure.Outbox;
using Shared.Infrastructure.Persistence;

namespace Shared.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // -------------------------------------------------------
        // Database
        // -------------------------------------------------------

        services.AddScoped<AuditInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var auditInterceptor = sp.GetRequiredService<AuditInterceptor>();

            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql =>
                {
                    sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                });

            options.AddInterceptors(auditInterceptor);
        });

        // Expose DbContext through abstraction
        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<AppDbContext>());

        // -------------------------------------------------------
        // Multi-Tenancy
        // -------------------------------------------------------

        services.AddScoped<ITenantProvider, HttpTenantProvider>();

        // -------------------------------------------------------
        // Domain Event Dispatching
        // -------------------------------------------------------

        services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();
        
        // Production will only scan Domain assembly
        services.AddSingleton(sp =>
            new DomainEventTypeRegistry(
                typeof(IDomainEvent).Assembly
            ));


        return services;
    }
}
