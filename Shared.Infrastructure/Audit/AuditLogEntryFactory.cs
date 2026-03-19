using System.Text.Json;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Primitives;
using Modules.Records.Domain.DomainEvents;

namespace Shared.Infrastructure.Audit;

internal static class AuditLogEntryFactory
{
    public static AuditLogReadModel CreateFromDomainEvent(
        IDomainEvent domainEvent,
        AggregateRoot? aggregate,
        Guid jurisdictionId)
    {
        var metadata = DescribeDomainEvent(domainEvent, aggregate);

        return AuditLogReadModel.Create(
            id: Guid.NewGuid(),
            eventId: domainEvent.EventId,
            eventType: domainEvent.GetType().Name,
            severity: metadata.Severity,
            occurredOnUtc: domainEvent.OccurredOnUtc,
            jurisdictionId: jurisdictionId,
            aggregateId: domainEvent.AggregateId,
            aggregateVersion: domainEvent.AggregateVersion,
            recordType: metadata.RecordType,
            actionType: metadata.ActionType,
            userId: metadata.UserId,
            message: metadata.Message,
            payload: JsonSerializer.Serialize(domainEvent, domainEvent.GetType()));
    }

    public static AuditLogReadModel CreateSystemLockExpired(
        Guid jurisdictionId,
        Guid aggregateId,
        long aggregateVersion,
        string recordType,
        DateTime occurredOnUtc,
        Guid lockedByUserId,
        DateTime lockedAtUtc)
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
            payload: payload);
    }

    private static AuditLogMetadata DescribeDomainEvent(IDomainEvent domainEvent, AggregateRoot? aggregate)
    {
        var type = domainEvent.GetType();

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(LockAcquiredDomainEvent<>))
        {
            var recordType = type.GenericTypeArguments[0].Name;
            var userId = ReadGuidProperty(domainEvent, "UserId");
            return new AuditLogMetadata("Information", recordType, "LockAcquired", userId, $"{recordType} lock acquired.");
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(LockReleasedDomainEvent<>))
        {
            var recordType = type.GenericTypeArguments[0].Name;
            var userId = ReadGuidProperty(domainEvent, "UserId");
            return new AuditLogMetadata("Information", recordType, "LockReleased", userId, $"{recordType} lock released.");
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(LifecycleStatusChangedDomainEvent<>))
        {
            var recordType = type.GenericTypeArguments[0].Name;
            var userId = ReadGuidProperty(domainEvent, "ChangedByUserId");
            var previous = ReadProperty(domainEvent, "PreviousStatus")?.ToString() ?? "Unknown";
            var next = ReadProperty(domainEvent, "NewStatus")?.ToString() ?? "Unknown";
            return new AuditLogMetadata(
                "Information",
                recordType,
                "StatusChanged",
                userId,
                $"{recordType} status changed from {previous} to {next}.");
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

            var userId = aggregate?.ModifiedBy ?? aggregate?.CreatedBy;
            return new AuditLogMetadata(
                suffix.Value.Severity,
                recordType,
                suffix.Value.ActionType,
                userId == Guid.Empty ? null : userId,
                $"{recordType} {suffix.Value.MessageVerb}.");
        }

        var fallbackUserId = aggregate?.ModifiedBy ?? aggregate?.CreatedBy;
        return new AuditLogMetadata(
            "Information",
            "System",
            eventName,
            fallbackUserId == Guid.Empty ? null : fallbackUserId,
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
        Guid? UserId,
        string Message);
}
