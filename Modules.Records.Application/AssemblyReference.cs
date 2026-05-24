using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Common;
using Modules.Records.Application.Common.Behaviors;

namespace Modules.Records.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
                // Demo abuse limits run first so an over-limit demo request is
                // rejected before any validation or handler work.
                cfg.AddOpenBehavior(typeof(DemoRateLimitBehavior<,>));
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            // Defaults apply unless a host binds the "Demo:RateLimit" section.
            services.AddOptions<DemoRateLimitOptions>();

            // Hosts (Shared.Infrastructure / UI) replace this with a real
            // implementation; the null default keeps consumers safe for tests
            // and background services that don't have a current user.
            services.TryAddScoped<ICurrentUserContext, NullCurrentUserContext>();

            return services;
        }
    }
}
