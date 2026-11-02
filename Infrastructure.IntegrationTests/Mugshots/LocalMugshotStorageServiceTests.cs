using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Mugshots;

namespace Infrastructure.IntegrationTests.Mugshots;

/// <summary>
/// File-system behavior of <see cref="LocalMugshotStorageService"/>: images are kept outside
/// the web root, keys written before the move still resolve, and keys can't escape the root.
/// </summary>
public sealed class LocalMugshotStorageServiceTests : IDisposable
{
    private static readonly byte[] Bytes = [1, 2, 3];

    private readonly string _contentRoot = Directory.CreateTempSubdirectory("mugshot-storage-tests-").FullName;

    [Fact]
    public async Task Save_WritesUnderConfiguredRoot_NotWebRoot()
    {
        var storage = CreateService("store");
        var jurisdictionId = Guid.NewGuid();
        var mugshotId = Guid.NewGuid();

        var result = await storage.SaveAsync(jurisdictionId, mugshotId, Bytes, "a.png", "image/png", CancellationToken.None);

        Assert.StartsWith($"{jurisdictionId:N}/", result.StoragePath);
        Assert.EndsWith($"{mugshotId:N}.png", result.StoragePath);
        Assert.True(File.Exists(Path.Combine(_contentRoot, "store", result.StoragePath)));
        Assert.False(Directory.Exists(Path.Combine(_contentRoot, "wwwroot", "uploads")));
        Assert.Equal(Bytes, await ReadAllAsync(storage, result.StoragePath));
    }

    [Fact]
    public async Task DefaultRoot_IsAppDataUnderContentRoot()
    {
        var storage = CreateService(configuredPath: null);

        var result = await storage.SaveAsync(Guid.NewGuid(), Guid.NewGuid(), Bytes, "a.jpg", "image/jpeg", CancellationToken.None);

        Assert.True(File.Exists(Path.Combine(_contentRoot, "App_Data", "mugshots", result.StoragePath)));
    }

    [Fact]
    public async Task LegacyKey_ResolvesUnderNewRoot()
    {
        // Production: the uploads volume moved from wwwroot/uploads to the new root's parent,
        // so legacy files sit at <root>/<path after uploads/mugshots/>.
        var relative = "j/2026/03/old.jpg";
        WriteFile(Path.Combine(_contentRoot, "store", relative));

        Assert.Equal(Bytes, await ReadAllAsync(CreateService("store"), "uploads/mugshots/" + relative));
    }

    [Fact]
    public async Task LegacyKey_FallsBackToOldWebRootLocation_AndCanBeDeleted()
    {
        var relative = "j/2026/03/old.jpg";
        var legacyFile = Path.Combine(_contentRoot, "wwwroot", "uploads", "mugshots", relative);
        WriteFile(legacyFile);
        var storage = CreateService("store");

        Assert.Equal(Bytes, await ReadAllAsync(storage, "uploads/mugshots/" + relative));

        await storage.DeleteAsync("uploads/mugshots/" + relative, CancellationToken.None);
        Assert.False(File.Exists(legacyFile));
    }

    [Theory]
    [InlineData("../outside.jpg")]
    [InlineData("uploads/mugshots/../../outside.jpg")]
    [InlineData("j/../../outside.jpg")]
    public async Task KeysEscapingTheRoot_AreRefused(string storagePath)
    {
        var outside = Path.Combine(_contentRoot, "outside.jpg");
        WriteFile(outside);
        var storage = CreateService("store");

        Assert.Null(await storage.OpenReadAsync(storagePath, CancellationToken.None));
        await storage.DeleteAsync(storagePath, CancellationToken.None);
        Assert.True(File.Exists(outside));
    }

    [Fact]
    public async Task MissingFile_ReturnsNull()
    {
        Assert.Null(await CreateService("store").OpenReadAsync("j/2026/03/missing.jpg", CancellationToken.None));
    }

    private LocalMugshotStorageService CreateService(string? configuredPath) => new(
        new TestWebHostEnvironment(_contentRoot),
        Options.Create(new MugshotStorageOptions { StoragePath = configuredPath }));

    private static async Task<byte[]?> ReadAllAsync(LocalMugshotStorageService storage, string storagePath)
    {
        await using var stream = await storage.OpenReadAsync(storagePath, CancellationToken.None);
        if (stream is null)
            return null;

        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer);
        return buffer.ToArray();
    }

    private static void WriteFile(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, Bytes);
    }

    public void Dispose() => Directory.Delete(_contentRoot, recursive: true);

    private sealed class TestWebHostEnvironment(string contentRoot) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Tests";
        public string EnvironmentName { get; set; } = "Testing";
        public string ContentRootPath { get; set; } = contentRoot;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = Path.Combine(contentRoot, "wwwroot");
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
