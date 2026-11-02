using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Modules.Records.Application.Abstractions;

namespace Shared.Infrastructure.Mugshots;

/// <summary>
/// Stores mugshot images on the local file system under
/// <see cref="MugshotStorageOptions.StoragePath"/>, keyed
/// <c>&lt;jurisdictionId&gt;/&lt;yyyy&gt;/&lt;MM&gt;/&lt;mugshotId&gt;.&lt;ext&gt;</c>.
/// </summary>
/// <remarks>
/// Mugshots uploaded before this layout were written to
/// <c>wwwroot/uploads/mugshots/...</c> with keys prefixed <c>uploads/mugshots/</c>. Those
/// keys resolve to the same relative path under the storage root (in production the
/// uploads volume is remounted there), falling back to the old web-root location.
/// </remarks>
public sealed class LocalMugshotStorageService : IMugshotStorageService
{
    internal const string LegacyKeyPrefix = "uploads/mugshots/";

    private readonly string _rootPath;
    private readonly string _legacyRootPath;

    public LocalMugshotStorageService(IWebHostEnvironment environment, IOptions<MugshotStorageOptions> options)
    {
        var configured = options.Value.StoragePath;
        _rootPath = Path.GetFullPath(string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(environment.ContentRootPath, "App_Data", "mugshots")
            : Path.Combine(environment.ContentRootPath, configured));

        var webRootPath = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        _legacyRootPath = Path.GetFullPath(Path.Combine(webRootPath, "uploads", "mugshots"));
    }

    public async Task<MugshotStorageSaveResult> SaveAsync(
        Guid jurisdictionId,
        Guid mugshotId,
        byte[] content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        var extension = ResolveExtension(contentType, fileName);
        var now = DateTime.UtcNow;
        var storageKey = string.Join('/',
            jurisdictionId.ToString("N"),
            now.Year.ToString(),
            now.Month.ToString("00"),
            $"{mugshotId:N}{extension}");

        var fullPath = ResolveUnderRoot(_rootPath, storageKey)
            ?? throw new InvalidOperationException("Could not resolve the mugshot storage path.");

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllBytesAsync(fullPath, content, cancellationToken);

        return new MugshotStorageSaveResult(storageKey, content.LongLength);
    }

    public Task<Stream?> OpenReadAsync(string storagePath, CancellationToken cancellationToken)
    {
        var fullPath = FindExistingFile(storagePath);
        Stream? stream = fullPath is null
            ? null
            : new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 81920, useAsync: true);

        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken)
    {
        var fullPath = FindExistingFile(storagePath);
        if (fullPath is not null)
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string? FindExistingFile(string storagePath)
    {
        if (string.IsNullOrWhiteSpace(storagePath))
            return null;

        var relativePath = storagePath.Replace('\\', '/');
        if (relativePath.StartsWith(LegacyKeyPrefix, StringComparison.OrdinalIgnoreCase))
            relativePath = relativePath[LegacyKeyPrefix.Length..];

        foreach (var root in new[] { _rootPath, _legacyRootPath })
        {
            var candidate = ResolveUnderRoot(root, relativePath);
            if (candidate is not null && File.Exists(candidate))
                return candidate;
        }

        return null;
    }

    /// <summary>Combines root and key, refusing keys that would escape the root.</summary>
    private static string? ResolveUnderRoot(string root, string relativePath)
    {
        var fullPath = Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        var rootWithSeparator = Path.EndsInDirectorySeparator(root) ? root : root + Path.DirectorySeparatorChar;
        return fullPath.StartsWith(rootWithSeparator, StringComparison.Ordinal) ? fullPath : null;
    }

    private static string ResolveExtension(string contentType, string fileName) =>
        contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => Path.GetExtension(fileName)
        };
}
