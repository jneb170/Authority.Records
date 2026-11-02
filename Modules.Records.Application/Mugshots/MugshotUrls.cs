namespace Modules.Records.Application.Mugshots;

/// <summary>
/// URLs under which mugshot images are served. Images are only available through the
/// authorized <c>/mugshots/{id}</c> endpoint, which applies the same tenant query
/// filters as the records they belong to; they are never served as static files.
/// </summary>
public static class MugshotUrls
{
    /// <summary>Route template the UI host maps the image endpoint to.</summary>
    public const string RouteTemplate = "/mugshots/{id:guid}";

    /// <summary>
    /// Prefix of the URLs mugshots had when they were served from wwwroot as static files.
    /// Still present in data written before the endpoint existed; rewritten on startup.
    /// </summary>
    public const string LegacyUrlPrefix = "/uploads/mugshots/";

    public static string For(Guid mugshotId) => $"/mugshots/{mugshotId:N}";
}
