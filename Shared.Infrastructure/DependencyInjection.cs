using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Policies;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Entities;
using Modules.Records.Domain.Factories;
using Modules.Records.Infrastructure.Services;
using MediatR;
using Shared.Infrastructure.Audit;
using Shared.Infrastructure.DomainEvents;
using Shared.Infrastructure.Identity;
using Shared.Infrastructure.Jurisdiction;
using Shared.Infrastructure.Outbox;
using Shared.Infrastructure.Persistence;
using System;

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
        services.AddTransient<INotificationHandler<IDomainEvent>, AuditTrailDomainEventHandler>();


        // -------------------------------------------------------
        // Outbox Message Cleanup Service
        // -------------------------------------------------------
        services.Configure<OutboxCleanupOptions>(configuration.GetSection("OutboxCleanup"));
        services.AddScoped<OutboxCleanupProcessor>();
        services.AddScoped<DeadLetterQueueWriter>();

        // -------------------------------------------------------
        // Domain Services / Repositories
        // -------------------------------------------------------
        services.AddScoped<IJurisdictionRulesService, JurisdictionRulesService>();
        services.AddScoped<IJurisdictionConfigurationRepository, JurisdictionConfigurationRepository>();

        // -------------------------------------------------------
        // Register Close Policies per Aggregate
        // -------------------------------------------------------
        services.AddScoped<IClosePolicy<Incident>>(sp =>
        {
            var jurisdictionRules = sp.GetRequiredService<IJurisdictionRulesService>();

            var policies = new IClosePolicy<Incident>[]
            {
                new IncidentClosePolicy(jurisdictionRules)
                //,new IncidentRequiredFieldsClosePolicy()
                //,new IncidentChildAggregateClosePolicy()
            };

            return new CompositeClosePolicy<Incident>(policies);
        });

        services.AddScoped<IClosePolicy<Arrest>>(sp =>
        {
            var jurisdictionRules = sp.GetRequiredService<IJurisdictionRulesService>();

            var policies = new IClosePolicy<Arrest>[]
            {
                new ArrestClosePolicy(jurisdictionRules)
                //,new ArrestRequiredFieldsClosePolicy()
            };

            return new CompositeClosePolicy<Arrest>(policies);
        });

        services.AddScoped<IClosePolicy<Citation>>(sp =>
        {
            var jurisdictionRules = sp.GetRequiredService<IJurisdictionRulesService>();

            var policies = new IClosePolicy<Citation>[]
            {
                new CitationClosePolicy(jurisdictionRules)
                //,new CitationRequiredFieldsClosePolicy()
            };

            return new CompositeClosePolicy<Citation>(policies);
        });

        // -------------------------------------------------------
        // Register Lifecycle Policies per Aggregate
        // -------------------------------------------------------
        services.AddScoped<ILifecyclePolicy<Incident>>(sp =>
        {
            var closePolicy = sp.GetRequiredService<IClosePolicy<Incident>>();
            return new DefaultLifecyclePolicy<Incident>(closePolicy);
            // Or new IncidentLifecyclePolicy(closePolicy) if we want to use the extension hook
        });

        services.AddScoped<ILifecyclePolicy<Arrest>>(sp =>
        {
            var closePolicy = sp.GetRequiredService<IClosePolicy<Arrest>>();
            return new DefaultLifecyclePolicy<Arrest>(closePolicy);
            // Or new ArrestLifecyclePolicy(closePolicy) if we want to use the extension hook
        });

        services.AddScoped<ILifecyclePolicy<Citation>>(sp =>
        {
            var closePolicy = sp.GetRequiredService<IClosePolicy<Citation>>();
            return new DefaultLifecyclePolicy<Citation>(closePolicy);
            // Or new CitationLifecyclePolicy(closePolicy) if we want to use the extension hook
        });

        // -------------------------------------------------------
        // Aggregates are created via Application Layer / Factories
        // -------------------------------------------------------
        services.AddScoped<IncidentFactory>();
        services.AddScoped<ArrestFactory>();


        // -------------------------------------------------------
        // Scan Domain Assembly for types implementing IDomainEvent.  
        // -------------------------------------------------------
        //Production will only scan Domain assembly, but in tests
        //  we may have test-specific events defined in the test
        //  assembly, so we can register those as well if needed
        services.AddSingleton(sp =>
            new DomainEventTypeRegistry(
                typeof(IDomainEvent).Assembly
            ));


        return services;
    }
}

