namespace Modules.Records.Application.Abstractions;

/// <summary>
/// Application-layer view of the currently authenticated user.
/// Lets handlers and pipeline behaviors check role membership without
/// taking a direct dependency on AspNetCore's HttpContext.
/// </summary>
public interface ICurrentUserContext
{
    bool IsInRole(string roleName);
}
