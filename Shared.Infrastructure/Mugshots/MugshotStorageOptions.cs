namespace Shared.Infrastructure.Mugshots;

public sealed class MugshotStorageOptions
{
    public const string SectionName = "Mugshots";

    /// <summary>
    /// Directory mugshot images are stored in. Must be outside the web root so files are
    /// only reachable through the authorized endpoint. Relative paths are resolved against
    /// the content root. Defaults to <c>App_Data/mugshots</c> under the content root.
    /// </summary>
    public string? StoragePath { get; set; }
}
