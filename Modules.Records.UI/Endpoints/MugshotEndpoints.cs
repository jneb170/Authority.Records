using MediatR;
using Modules.Records.Application.Mugshots;
using Modules.Records.Application.Mugshots.Queries.GetMugshotContent;

namespace Modules.Records.UI.Endpoints;

/// <summary>
/// Serves mugshot images. Like the record pages (Pages/_Imports.razor), it requires a
/// signed-in user; tenant isolation comes from the query filters behind
/// <see cref="GetMugshotContentQuery"/>, so another jurisdiction's mugshot is a 404.
/// </summary>
public static class MugshotEndpoints
{
    public static IEndpointConventionBuilder MapMugshotEndpoints(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(MugshotUrls.RouteTemplate, GetMugshotAsync)
            .RequireAuthorization();

    /// <summary>
    /// Mugshots used to be written to <c>wwwroot/uploads</c> and served as public static
    /// files. They now live outside the web root, but refuse the old path outright in case
    /// files remain there (e.g. a development checkout). Must run before static files.
    /// </summary>
    public static IApplicationBuilder BlockLegacyUploadPaths(this IApplicationBuilder app) =>
        app.Use((context, next) =>
        {
            if (context.Request.Path.StartsWithSegments("/uploads", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return Task.CompletedTask;
            }

            return next(context);
        });

    private static async Task<IResult> GetMugshotAsync(
        Guid id,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var mugshot = await sender.Send(new GetMugshotContentQuery(id), cancellationToken);
        if (mugshot is null)
            return Results.NotFound();

        // Browser-only caching: never stored by shared caches/proxies, and short-lived so
        // a revoked user doesn't keep seeing images for long.
        httpContext.Response.Headers.CacheControl = "private, max-age=300";
        httpContext.Response.Headers.XContentTypeOptions = "nosniff";

        // Results.File disposes the stream once the response is written.
        return Results.File(mugshot.Content, mugshot.ContentType);
    }
}
