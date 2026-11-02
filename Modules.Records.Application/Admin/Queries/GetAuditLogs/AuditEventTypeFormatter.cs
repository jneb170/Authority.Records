namespace Modules.Records.Application.Admin.Queries.GetAuditLogs;

/// <summary>
/// Turns domain event CLR type names into the readable event names shown in the audit log,
/// e.g. <c>LifecycleStatusChangedDomainEvent`1</c> for an incident becomes <c>IncidentStatusChanged</c>
/// and <c>NameUpdatedDomainEvent</c> becomes <c>NameUpdated</c> (issue #119).
/// </summary>
public static class AuditEventTypeFormatter
{
    private const string DomainEventSuffix = "DomainEvent";

    /// <summary>Readable name for a domain event type, as stored on new audit rows.</summary>
    public static string FromType(Type eventType)
    {
        if (!eventType.IsGenericType)
            return Format(eventType.Name, recordType: null);

        var recordType = eventType.GenericTypeArguments.Length > 0
            ? eventType.GenericTypeArguments[0].Name
            : null;

        return Format(eventType.Name, recordType);
    }

    /// <summary>
    /// Readable name for a stored event type. Rows written before #119 hold raw CLR names
    /// (<c>LockAcquiredDomainEvent`1</c>), so the record type is passed in to qualify them.
    /// </summary>
    public static string Format(string storedEventType, string? recordType)
    {
        if (string.IsNullOrWhiteSpace(storedEventType))
            return storedEventType;

        var name = storedEventType;
        var isGeneric = false;

        var backtick = name.IndexOf('`');
        if (backtick >= 0)
        {
            name = name[..backtick];
            isGeneric = true;
        }

        if (name.EndsWith(DomainEventSuffix, StringComparison.Ordinal) && name.Length > DomainEventSuffix.Length)
            name = name[..^DomainEventSuffix.Length];

        if (!isGeneric)
            return name;

        if (name == "LifecycleStatusChanged")
            name = "StatusChanged";

        return string.IsNullOrWhiteSpace(recordType) || recordType == "System"
            ? name
            : recordType + name;
    }
}
