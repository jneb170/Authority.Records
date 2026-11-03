using System.Collections.Concurrent;
using System.Reflection;
using MediatR;
using Microsoft.Extensions.Options;
using Modules.Records.Application.Abstractions;

namespace Modules.Records.Application.Common.Behaviors;

/// <summary>
/// Enforces <see cref="RequiresRoleAttribute"/> and <see cref="DemoOnlyAttribute"/> on requests
/// before any other behavior or the handler runs (issue #125).
/// </summary>
public sealed class AuthorizationBehavior<TRequest, TResponse>(
    ICurrentUserContext currentUser,
    IOptions<DemoModeOptions> demoMode)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly ConcurrentDictionary<Type, (RequiresRoleAttribute? Roles, bool DemoOnly)> Rules = new();

    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var (roles, demoOnly) = Rules.GetOrAdd(typeof(TRequest), static type => (
            type.GetCustomAttribute<RequiresRoleAttribute>(),
            type.GetCustomAttribute<DemoOnlyAttribute>() is not null));

        if (demoOnly && !demoMode.Value.Enabled)
            throw new UnauthorizedAccessException("Test data can only be generated in the demo.");

        if (roles is not null && !roles.Roles.Any(currentUser.IsInRole))
            throw new UnauthorizedAccessException("You don't have permission to do this.");

        return next();
    }
}
