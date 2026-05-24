using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Common;

namespace Shared.Infrastructure.Identity;

public sealed class HttpCurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _demoEmail;

    public HttpCurrentUserContext(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _demoEmail = DemoUserMatching.ResolveDemoEmail(configuration[DemoUserMatching.LoginEmailConfigKey]);
    }

    public bool IsInRole(string roleName)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.IsInRole(roleName) ?? false;
    }

    public bool IsDemoUser =>
        DemoUserMatching.IsDemoPrincipal(_httpContextAccessor.HttpContext?.User, _demoEmail);
}
