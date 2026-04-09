namespace Modules.Records.Application.DTOs;

public sealed record NameSnapshotAddressDto(
    Guid? LocationId,
    long? LocationRecordNumber,
    string? Address);
