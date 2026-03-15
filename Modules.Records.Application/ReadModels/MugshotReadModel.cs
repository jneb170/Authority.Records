using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.ReadModels;

public sealed class MugshotReadModel
{
    public Guid Id { get; private set; }
    public Guid JurisdictionId { get; private set; }
    public Guid AgencyId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }
    public string StoragePath { get; private set; } = string.Empty;
    public string PublicUrl { get; private set; } = string.Empty;
    public DateTime CapturedAtUtc { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private MugshotReadModel()
    {
    }

    public static MugshotReadModel Create(
        Guid id,
        Guid jurisdictionId,
        Guid agencyId,
        string fileName,
        string contentType,
        long fileSizeBytes,
        string storagePath,
        string publicUrl,
        DateTime capturedAtUtc,
        Guid createdBy,
        DateTime createdAtUtc)
    {
        return new MugshotReadModel
        {
            Id = id,
            JurisdictionId = jurisdictionId,
            AgencyId = agencyId,
            FileName = fileName,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            StoragePath = storagePath,
            PublicUrl = publicUrl,
            CapturedAtUtc = capturedAtUtc,
            CreatedBy = createdBy,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc
        };
    }

    public MugshotDto ToDto(MugshotLinkReadModel link) => new(
        Id,
        link.OwnerType,
        link.OwnerId,
        FileName,
        ContentType,
        FileSizeBytes,
        PublicUrl,
        CapturedAtUtc,
        link.LinkedAtUtc,
        link.IsPrimary,
        link.DisplayOrder);
}
