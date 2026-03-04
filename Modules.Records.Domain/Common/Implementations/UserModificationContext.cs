using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Domain.Common.Implementations;

public sealed class UserModificationContext : IModificationContext
{
    public Guid UserId { get; }
    public bool CanOverrideLocks { get; }
    public bool CanModifyClosedRecords { get; }
    public bool IsSystem { get; }

    public UserModificationContext(
        Guid userId,
        bool canOverrideLocks = false,
        bool canModifyClosedRecords = false,
        bool isSystem = false)
    {
        UserId = userId;
        CanOverrideLocks = canOverrideLocks;
        CanModifyClosedRecords = canModifyClosedRecords;
        IsSystem = isSystem;
    }
}