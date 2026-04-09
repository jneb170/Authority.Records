using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Primitives;
using Modules.Records.Domain.DomainEvents;

namespace Modules.Records.Domain.Entities;

public sealed class Mugshot : AggregateRoot, IMultiTenant
{
    public Guid JurisdictionId { get; private set; }
    public Guid AgencyId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }
    public string StoragePath { get; private set; } = string.Empty;
    public string PublicUrl { get; private set; } = string.Empty;
    public DateTime CapturedAtUtc { get; private set; }

    private Mugshot()
    {
    }

    public Mugshot(
        Guid jurisdictionId,
        Guid agencyId,
        string fileName,
        string contentType,
        long fileSizeBytes,
        string storagePath,
        string publicUrl,
        DateTime capturedAtUtc)
    {
        Id = Guid.NewGuid();
        JurisdictionId = jurisdictionId;
        AgencyId = agencyId;
        FileName = fileName;
        ContentType = contentType;
        FileSizeBytes = fileSizeBytes;
        StoragePath = storagePath;
        PublicUrl = publicUrl;
        CapturedAtUtc = capturedAtUtc;

        AddDomainEvent(new MugshotCreatedDomainEvent(
            Id,
            JurisdictionId,
            FileName,
            ContentType,
            FileSizeBytes,
            PublicUrl,
            CapturedAtUtc));
    }

    public override void SoftDelete(Guid userId)
    {
        base.SoftDelete(userId);
        AddDomainEvent(new MugshotSoftDeletedDomainEvent(Id, userId));
    }

    public override void Restore(Guid userId)
    {
        base.Restore(userId);
        AddDomainEvent(new MugshotRestoredDomainEvent(Id, userId));
    }
}
