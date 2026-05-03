using MediatR;
using Modules.Records.Application.Abstractions;

namespace Modules.Records.Application.Common.Behaviors;

public sealed class DemoUserWriteGuardBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const string DemoRoleName = "Demo";

    private readonly ICurrentUserContext _currentUser;

    public DemoUserWriteGuardBehavior(ICurrentUserContext currentUser)
    {
        _currentUser = currentUser;
    }

    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Convention: every write request type is named *Command and lives
        // under .../Commands/. Queries end in *Query. This is uniform across
        // the codebase as of this writing — see ARCHITECTURE.md and any
        // request file under Modules.Records.Application.
        var requestTypeName = typeof(TRequest).Name;
        if (requestTypeName.EndsWith("Command", StringComparison.Ordinal)
            && _currentUser.IsInRole(DemoRoleName))
        {
            throw new DemoWriteForbiddenException(requestTypeName);
        }

        return next();
    }
}
