namespace Modules.Records.Application.Abstractions;

/// <summary>
/// Resolves display names for users by ID.
/// Implemented in infrastructure to keep Application layer free of Identity dependencies.
/// </summary>
public interface IUserLookupService
{
    /// <summary>
    /// Returns the full name ("First Last") for <paramref name="userId"/>,
    /// falling back to email/username if the name is not set. Returns <c>null</c> if not found.
    /// </summary>
    Task<string?> GetDisplayNameAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk lookup — returns a dictionary of userId → display name for the given IDs.
    /// Missing or unknown users are omitted from the result.
    /// </summary>
    Task<Dictionary<Guid, string>> GetDisplayNamesAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
}
