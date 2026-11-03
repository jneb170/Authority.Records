using System.Text.Json;
using Modules.Records.Application.Admin.Queries.GetAuditLogs;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Primitives;
using Modules.Records.Domain.DomainEvents;

namespace Shared.Infrastructure.Audit;

internal static class AuditLogEntryFactory
{
    /// <summary>
    /// Event properties that name the user who performed the action, checked in order.
    /// Lock and lifecycle events carry the actor here; so do delete/restore/link events.
    /// </summary>
    private static readonly string[] ActorPropertyNames =
    [
        "UserId",
        "ChangedByUserId",
        "DeletedByUserId",
        "RestoredByUserId",
        "IssuedByUserId",
        "LinkedByUserId",
        "UnlinkedByUserId"
    ];

    /// <summary>
    /// Builds the audit row for a domain event.
    /// </summary>
    /// <param name="actorUserId">
    /// The user executing the current save (from the request's claims), used when the event
    /// itself does not name its actor. The row is never attributed from the aggregate's
    /// CreatedBy/ModifiedBy stamps: those are only written by the save interceptor after this
    /// row is built, so they still hold the <em>previous</em> modifier (issue #119).
    /// </param>
    /// <param name="actorDisplayNames">Display names for the actors in this save, keyed by user ID.</param>
    public static AuditLogReadModel CreateFromDomainEvent(
        IDomainEvent domainEvent,
        Guid jurisdictionId,
        Guid? actorUserId,
        IReadOnlyDictionary<Guid, string>? actorDisplayNames = null,
        Guid? agencyId = null)
    {
        var metadata = DescribeDomainEvent(domainEvent);
        var userId = ResolveActor(domainEvent, actorUserId);

        string? displayName = null;
        if (userId.HasValue && actorDisplayNames is not null)
            actorDisplayNames.TryGetValue(userId.Value, out displayName);

        return AuditLogReadModel.Create(
            id: Guid.NewGuid(),
            eventId: domainEvent.EventId,
            eventType: AuditEventTypeFormatter.FromType(domainEvent.GetType()),
            severity: metadata.Severity,
            occurredOnUtc: domainEvent.OccurredOnUtc,
            jurisdictionId: jurisdictionId,
            aggregateId: domainEvent.AggregateId,
            aggregateVersion: domainEvent.AggregateVersion,
            recordType: metadata.RecordType,
            actionType: metadata.ActionType,
            userId: userId,
            message: metadata.Message,
            payload: JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
            userDisplayName: displayName,
            agencyId: agencyId);
    }

    /// <summary>
    /// The actor for an event: the user the event names, else the user executing the save,
    /// else null (a genuine system action, shown as "System").
    /// </summary>
    public static Guid? ResolveActor(IDomainEvent domainEvent, Guid? actorUserId)
    {
        foreach (var propertyName in ActorPropertyNames)
        {
            var explicitActor = ReadGuidProperty(domainEvent, propertyName);
            if (explicitActor.HasValue)
                return explicitActor;
        }

        return actorUserId.HasValue && actorUserId.Value != Guid.Empty ? actorUserId : null;
    }

    public static AuditLogReadModel CreateSystemLockExpired(
        Guid jurisdictionId,
        Guid aggregateId,
        long aggregateVersion,
        string recordType,
        DateTime occurredOnUtc,
        Guid lockedByUserId,
        DateTime lockedAtUtc,
        Guid? agencyId = null)
    {
        var payload = JsonSerializer.Serialize(new
        {
            AggregateType = recordType,
            LockedByUserId = lockedByUserId,
            LockedAtUtc = lockedAtUtc,
            ExpiredAtUtc = occurredOnUtc
        });

        return AuditLogReadModel.Create(
            id: Guid.NewGuid(),
            eventId: Guid.NewGuid(),
            eventType: "SystemLockExpired",
            severity: "Warning",
            occurredOnUtc: occurredOnUtc,
            jurisdictionId: jurisdictionId,
            aggregateId: aggregateId,
            aggregateVersion: aggregateVersion,
            recordType: recordType,
            actionType: "LockExpired",
            userId: null,
            message: $"System released an expired {recordType.ToLowerInvariant()} lock.",
            payload: payload,
            agencyId: agencyId);
    }

    private static AuditLogMetadata DescribeDomainEvent(IDomainEvent domainEvent)
    {
        var type = domainEvent.GetType();

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(LockAcquiredDomainEvent<>))
        {
            var recordType = type.GenericTypeArguments[0].Name;
            return new AuditLogMetadata("Information", recordType, "LockAcquired", $"{recordType} lock acquired.");
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(LockReleasedDomainEvent<>))
        {
            var recordType = type.GenericTypeArguments[0].Name;
            return new AuditLogMetadata("Information", recordType, "LockReleased", $"{recordType} lock released.");
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(LifecycleStatusChangedDomainEvent<>))
        {
            var recordType = type.GenericTypeArguments[0].Name;
            var previous = ReadProperty(domainEvent, "PreviousStatus")?.ToString() ?? "Unknown";
            var next = ReadProperty(domainEvent, "NewStatus")?.ToString() ?? "Unknown";
            return new AuditLogMetadata(
                "Information",
                recordType,
                "StatusChanged",
                $"{recordType} status changed from {previous} to {next}.");
        }

        // Settings changes name the key and both values, e.g. TimeZoneId (#125).
        if (domainEvent is AgencyConfigurationSetDomainEvent set)
        {
            return set.PreviousValue is null
                ? new AuditLogMetadata("Information", "Configuration", "Created", $"Configuration {set.Key} set to \"{set.Value}\".")
                : new AuditLogMetadata("Warning", "Configuration", "Updated", $"Configuration {set.Key} changed from \"{set.PreviousValue}\" to \"{set.Value}\".");
        }

        if (domainEvent is AgencyConfigurationDeletedDomainEvent deleted)
        {
            return new AuditLogMetadata("Warning", "Configuration", "Deleted",
                $"Configuration {deleted.Key} deleted (was \"{deleted.PreviousValue}\"); the default now applies.");
        }

        var eventName = type.Name.EndsWith("DomainEvent", StringComparison.Ordinal)
            ? type.Name[..^"DomainEvent".Length]
            : type.Name;

        foreach (var suffix in KnownActionSuffixes)
        {
            if (!eventName.EndsWith(suffix.Key, StringComparison.Ordinal))
                continue;

            var recordType = eventName[..^suffix.Key.Length];
            if (string.IsNullOrWhiteSpace(recordType))
                break;

            return new AuditLogMetadata(
                suffix.Value.Severity,
                recordType,
                suffix.Value.ActionType,
                $"{recordType} {suffix.Value.MessageVerb}.");
        }

        return new AuditLogMetadata(
            "Information",
            "System",
            eventName,
            eventName);
    }

    private static object? ReadProperty(object target, string propertyName) =>
        target.GetType().GetProperty(propertyName)?.GetValue(target);

    private static Guid? ReadGuidProperty(object target, string propertyName)
    {
        var value = ReadProperty(target, propertyName);

        if (value is Guid guid && guid != Guid.Empty)
        {
            return guid;
        }

        return null;
    }

    private static readonly IReadOnlyDictionary<string, (string ActionType, string MessageVerb, string Severity)> KnownActionSuffixes =
        new Dictionary<string, (string ActionType, string MessageVerb, string Severity)>(StringComparer.Ordinal)
        {
            ["DetailsUpdated"] = ("Updated", "updated", "Information"),
            ["Updated"] = ("Updated", "updated", "Information"),
            ["Created"] = ("Created", "created", "Information"),
            ["Issued"] = ("Issued", "issued", "Information"),
            ["SoftDeleted"] = ("SoftDeleted", "soft deleted", "Warning"),
            ["Restored"] = ("Restored", "restored", "Information")
        };

    private sealed record AuditLogMetadata(
        string Severity,
        string RecordType,
        string ActionType,
        string Message);
}
