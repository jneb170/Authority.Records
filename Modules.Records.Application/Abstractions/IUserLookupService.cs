namespace Modules.Records.Application.Abstractions;

/// <summary>
/// Resolves a display name (email / username) for a given user ID.
/// Implemented in infrastructure to keep Application layer free of Identity dependencies.
/// </summary>
public interface IUserLookupService
{
    /// <summary>
    /// Returns the display name for <paramref name="userId"/>, or <c>null</c> if not found.
    /// </summary>
    Task<string?> GetDisplayNameAsync(Guid userId, CancellationToken cancellationToken = default);
}
