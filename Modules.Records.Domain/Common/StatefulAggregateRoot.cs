namespace Modules.Records.Domain.Common;

public abstract class StatefulAggregateRoot : AggregateRoot
{
    public RecordStatus Status { get; protected set; } = RecordStatus.Draft;

    protected void Open(Guid userId)
    {
        EnsureNotArchived();

        if (Status != RecordStatus.Draft)
            throw new DomainException("record.invalid.transition", "Only Draft records can be opened.");

        Status = RecordStatus.Open;

        AddDomainEvent(CreateOpenedEvent(userId));
    }

    protected void Close(Guid userId, bool isForced = false)
    {
        EnsureNotArchived();

        if (Status != RecordStatus.Open)
            throw new DomainException("record.invalid.transition", "Only Open records can be closed.");

        if (!isForced)
            ValidateForClose();

        Status = RecordStatus.Closed;

        AddDomainEvent(CreateClosedEvent(userId, isForced));
    }

    protected void Archive(Guid userId)
    {
        if (Status != RecordStatus.Closed)
            throw new DomainException("record.invalid.transition", "Only Closed records can be archived.");

        Status = RecordStatus.Archived;

        AddDomainEvent(CreateArchivedEvent(userId));
    }

    protected virtual void EnsureCanModify(Guid userId)
    {
        if (Status == RecordStatus.Archived)
            throw new DomainException("record.archived", "Archived record cannot be modified.");

        if (Status == RecordStatus.Closed)
            throw new DomainException("record.closed", "Closed record cannot be modified.");
    }

    protected virtual void EnsureNotArchived()
    {
        if (Status == RecordStatus.Archived)
            throw new DomainException("record.archived", "Archived record cannot be modified.");
    }

    protected virtual void ValidateForClose() { }

    protected abstract object CreateOpenedEvent(Guid userId);
    protected abstract object CreateClosedEvent(Guid userId, bool forced);
    protected abstract object CreateArchivedEvent(Guid userId);
}

