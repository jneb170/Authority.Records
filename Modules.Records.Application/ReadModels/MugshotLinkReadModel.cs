namespace Modules.Records.Application.ReadModels;

public sealed class MugshotLinkReadModel
{
    public Guid Id { get; private set; }
    public Guid JurisdictionId { get; private set; }
    public Guid MugshotId { get; private set; }
    public string OwnerType { get; private set; } = string.Empty;
    public Guid OwnerId { get; private set; }
    public bool IsPrimary { get; private set; }
    public int DisplayOrder { get; private set; }
    public DateTime LinkedAtUtc { get; private set; }

    private MugshotLinkReadModel()
    {
    }

    public static MugshotLinkReadModel Create(
        Guid id,
        Guid jurisdictionId,
        Guid mugshotId,
        string ownerType,
        Guid ownerId,
        bool isPrimary,
        int displayOrder,
        DateTime linkedAtUtc)
    {
        return new MugshotLinkReadModel
        {
            Id = id,
            JurisdictionId = jurisdictionId,
            MugshotId = mugshotId,
            OwnerType = ownerType,
            OwnerId = ownerId,
            IsPrimary = isPrimary,
            DisplayOrder = displayOrder,
            LinkedAtUtc = linkedAtUtc
        };
    }

    public void ApplyPrimaryChanged(bool isPrimary)
    {
        IsPrimary = isPrimary;
    }
}
