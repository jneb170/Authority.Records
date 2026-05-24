using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.ReadModels;

public sealed class NarrativeReadModel
{
    public Guid Id { get; private set; }
    public long RecordNumber { get; private set; }
    public Guid JurisdictionId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public bool IsLocked { get; private set; }
    public Guid? LockedByUserId { get; private set; }
    public Guid CreatedBy { get; private set; }
    public Guid? ModifiedBy { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private NarrativeReadModel() { } // EF Core materialization

    public static NarrativeReadModel Create(
        Guid id,
        long recordNumber,
        Guid jurisdictionId,
        string title,
        string content,
        DateTime createdAtUtc,
        Guid createdBy)
    {
        return new NarrativeReadModel
        {
            Id             = id,
            RecordNumber   = recordNumber,
            JurisdictionId = jurisdictionId,
            Title          = title,
            Content        = content,
            IsLocked       = false,
            CreatedBy      = createdBy,
            CreatedAtUtc   = createdAtUtc,
            UpdatedAtUtc   = createdAtUtc,
        };
    }

    public void ApplyContentChanged(string title, string content)
    {
        Title        = title;
        Content      = content;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ApplyLockAcquired(Guid userId)
    {
        IsLocked       = true;
        LockedByUserId = userId;
    }

    public void ApplyLockReleased()
    {
        IsLocked       = false;
        LockedByUserId = null;
    }

    public void ApplyModifiedAudit(Guid? modifiedBy, DateTime? modifiedAt)
    {
        ModifiedBy   = modifiedBy;
        UpdatedAtUtc = modifiedAt ?? UpdatedAtUtc;
    }

    public NarrativeDto ToDto() => new(
        Id, RecordNumber, JurisdictionId, Title, Content,
        IsLocked, LockedByUserId, CreatedBy, ModifiedBy, CreatedAtUtc, UpdatedAtUtc);
}
