namespace Modules.Records.Domain.DomainEvents;

public sealed record MugshotCreatedDomainEvent(
    Guid MugshotId,
    Guid JurisdictionId,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    string PublicUrl,
    DateTime CapturedAtUtc) : DomainEvent;
