namespace Modules.Records.Application.DTOs;

/// <summary>Jurisdiction-neutral offense detail returned with a citation. See <see cref="CitationOffenseDetailsInput"/>.</summary>
public sealed record CitationOffenseDetailsDto(
    Guid? ViolationSourceTypeId = null,
    string? ViolationSection = null,
    Guid? ViolationGroupId = null,
    string? PrimaryViolationDescription = null,
    int? SpeedMph = null,
    int? ZoneMph = null,
    Guid? SpeedBandId = null,
    string? NarrativeOtherViolations = null,
    string? OccurredAtText = null,
    DateTime? CourtAppearanceDateTime = null,
    Guid? CourtAppearanceLocationId = null,
    DateTime? AffidavitSignedDate = null,
    string? ComplainantSignatureText = null,
    string? DefendantSignatureText = null,
    string? AcceptedBondNotes = null,
    string? ReceiptNumber = null);
