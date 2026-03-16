using Microsoft.AspNetCore.Hosting;
using Modules.Records.Application.Abstractions;

namespace Shared.Infrastructure.Mugshots;

public sealed class LocalMugshotStorageService : IMugshotStorageService
{
    private readonly IWebHostEnvironment _environment;

    public LocalMugshotStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<MugshotStorageSaveResult> SaveAsync(
        Guid jurisdictionId,
        byte[] content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        var extension = ResolveExtension(contentType, fileName);
        var now = DateTime.UtcNow;
        var relativeDirectory = Path.Combine(
            "uploads",
            "mugshots",
            jurisdictionId.ToString("N"),
            now.Year.ToString(),
            now.Month.ToString("00"));

        var relativePath = Path.Combine(relativeDirectory, $"{Guid.NewGuid():N}{extension}");
        var webRootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var fullPath = Path.Combine(webRootPath, relativePath);
        var directory = Path.GetDirectoryName(fullPath)
            ?? throw new InvalidOperationException("Could not resolve the mugshot storage directory.");

        Directory.CreateDirectory(directory);
        await File.WriteAllBytesAsync(fullPath, content, cancellationToken);

        var normalizedPath = relativePath.Replace('\\', '/');
        return new MugshotStorageSaveResult(
            normalizedPath,
            $"/{normalizedPath}",
            content.LongLength);
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(storagePath))
        {
            return Task.CompletedTask;
        }

        var webRootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var fullPath = Path.Combine(webRootPath, storagePath.Replace('/', Path.DirectorySeparatorChar));

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
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
