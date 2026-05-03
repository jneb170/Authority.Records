using Microsoft.AspNetCore.Http;
using Modules.Records.Application.Abstractions;

namespace Shared.Infrastructure.Identity;

public sealed class HttpCurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsInRole(string roleName)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.IsInRole(roleName) ?? false;
    }
}
