namespace Modules.Records.UI.Middleware;

public sealed class CanonicalHostRedirectMiddleware
{
    private const string CanonicalHost = "authorityrecords.dev";
    private const string NonCanonicalHost = "www.authorityrecords.dev";

    private readonly RequestDelegate _next;

    public CanonicalHostRedirectMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        if (string.Equals(context.Request.Host.Host, NonCanonicalHost, StringComparison.OrdinalIgnoreCase))
        {
            var target = $"https://{CanonicalHost}{context.Request.PathBase}{context.Request.Path}{context.Request.QueryString}";
            context.Response.Headers.Location = target;
            context.Response.StatusCode = StatusCodes.Status301MovedPermanently;
            return Task.CompletedTask;
        }

        return _next(context);
    }
}
