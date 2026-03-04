namespace Modules.Records.Domain.Abstractions;

public interface IModificationContext
{
    Guid UserId { get; }

    bool CanOverrideLocks { get; }

    bool CanModifyClosedRecords { get; }

    bool IsSystem { get; }
}