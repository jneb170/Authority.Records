namespace Modules.Records.Application.DTOs;

public sealed record CitationTexasDetailsInput(
    string? DocketNumber = null,
    string? PageNumber = null,
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
