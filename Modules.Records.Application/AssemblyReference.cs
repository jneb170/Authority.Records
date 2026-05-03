using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Modules.Records.Application.Abstractions;
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
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                cfg.AddOpenBehavior(typeof(DemoUserWriteGuardBehavior<,>));
            });

            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            // Hosts (Shared.Infrastructure / UI) replace this with a real
            // implementation; the null default keeps the Demo write guard safe
            // for tests and background services that don't have a current user.
            services.TryAddScoped<ICurrentUserContext, NullCurrentUserContext>();

            return services;
        }
    }
}
