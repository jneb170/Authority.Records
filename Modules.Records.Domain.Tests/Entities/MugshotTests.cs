using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Tests.Entities;

public sealed class MugshotTests
{
    private static readonly Guid TestJurisdictionId = Guid.NewGuid();
    private static readonly Guid TestAgencyId = Guid.NewGuid();
    private static readonly Guid TestUserId = Guid.NewGuid();

    private static Mugshot CreateMugshot(
        string? fileName = null,
        string? contentType = null,
        long fileSizeBytes = 1024,
        string? storagePath = null,
        string? publicUrl = null,
        DateTime? capturedAtUtc = null) =>
        new Mugshot(
            TestJurisdictionId,
            TestAgencyId,
            fileName ?? "mugshot.jpg",
            contentType ?? "image/jpeg",
            fileSizeBytes,
            storagePath ?? "photos/mugshot.jpg",
            publicUrl ?? "https://cdn.example.com/mugshot.jpg",
            capturedAtUtc ?? DateTime.UtcNow);

    #region Constructor Tests

    [Fact]
    public void Constructor_WithRequiredFields_SetsProperties()
    {
        var capturedAt = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        var mugshot = new Mugshot(
            TestJurisdictionId,
            TestAgencyId,
            "photo.jpg",
            "image/jpeg",
            2048,
            "storage/photo.jpg",
            "https://example.com/photo.jpg",
            capturedAt);

        Assert.NotEqual(Guid.Empty, mugshot.Id);
        Assert.Equal(TestJurisdictionId, mugshot.JurisdictionId);
        Assert.Equal(TestAgencyId, mugshot.AgencyId);
        Assert.Equal("photo.jpg", mugshot.FileName);
        Assert.Equal("image/jpeg", mugshot.ContentType);
        Assert.Equal(2048, mugshot.FileSizeBytes);
        Assert.Equal("storage/photo.jpg", mugshot.StoragePath);
        Assert.Equal("https://example.com/photo.jpg", mugshot.PublicUrl);
        Assert.Equal(capturedAt, mugshot.CapturedAtUtc);
    }

    [Fact]
    public void Constructor_IsNotDeleted()
    {
        var mugshot = CreateMugshot();

        Assert.False(mugshot.IsDeleted);
    }

    [Fact]
    public void Constructor_RaisesMugshotCreatedDomainEvent()
    {
        var capturedAt = DateTime.UtcNow;
        var mugshot = new Mugshot(
            TestJurisdictionId,
            TestAgencyId,
            "photo.jpg",
            "image/png",
            512,
            "path/photo.jpg",
            "https://cdn.example.com/photo.jpg",
            capturedAt);

        var evt = Assert.Single(mugshot.DomainEvents.OfType<MugshotCreatedDomainEvent>());
        Assert.Equal(mugshot.Id, evt.MugshotId);
        Assert.Equal(TestJurisdictionId, evt.JurisdictionId);
        Assert.Equal("photo.jpg", evt.FileName);
        Assert.Equal("image/png", evt.ContentType);
        Assert.Equal(512, evt.FileSizeBytes);
        Assert.Equal("https://cdn.example.com/photo.jpg", evt.PublicUrl);
        Assert.Equal(capturedAt, evt.CapturedAtUtc);
    }

    [Fact]
    public void Constructor_ImplementsIMultiTenant()
    {
        var mugshot = CreateMugshot();

        Assert.IsAssignableFrom<IMultiTenant>(mugshot);
        Assert.Equal(TestJurisdictionId, mugshot.JurisdictionId);
        Assert.Equal(TestAgencyId, mugshot.AgencyId);
    }

    #endregion

    #region SoftDelete Tests

    [Fact]
    public void SoftDelete_MarksRecordAsDeleted()
    {
        var mugshot = CreateMugshot();

        mugshot.SoftDelete(TestUserId);

        Assert.True(mugshot.IsDeleted);
    }

    [Fact]
    public void SoftDelete_RaisesMugshotSoftDeletedDomainEvent()
    {
        var mugshot = CreateMugshot();
        mugshot.ClearDomainEvents();

        mugshot.SoftDelete(TestUserId);

        var evt = Assert.Single(mugshot.DomainEvents.OfType<MugshotSoftDeletedDomainEvent>());
        Assert.Equal(mugshot.Id, evt.MugshotId);
        Assert.Equal(TestUserId, evt.DeletedByUserId);
    }

    [Fact]
    public void SoftDelete_CalledTwice_RemainsDeleted()
    {
        var mugshot = CreateMugshot();

        mugshot.SoftDelete(TestUserId);
        mugshot.SoftDelete(TestUserId);

        Assert.True(mugshot.IsDeleted);
    }

    #endregion

    #region Restore Tests

    [Fact]
    public void Restore_ClearsDeletedState()
    {
        var mugshot = CreateMugshot();
        mugshot.SoftDelete(TestUserId);

        mugshot.Restore(TestUserId);

        Assert.False(mugshot.IsDeleted);
    }

    [Fact]
    public void Restore_RaisesMugshotRestoredDomainEvent()
    {
        var mugshot = CreateMugshot();
        mugshot.SoftDelete(TestUserId);
        mugshot.ClearDomainEvents();

        mugshot.Restore(TestUserId);

        var evt = Assert.Single(mugshot.DomainEvents.OfType<MugshotRestoredDomainEvent>());
        Assert.Equal(mugshot.Id, evt.MugshotId);
        Assert.Equal(TestUserId, evt.RestoredByUserId);
    }

    [Fact]
    public void SoftDelete_Then_Restore_LeavesRecordActive()
    {
        var mugshot = CreateMugshot();

        mugshot.SoftDelete(TestUserId);
        mugshot.Restore(TestUserId);

        Assert.False(mugshot.IsDeleted);
    }

    #endregion
}
