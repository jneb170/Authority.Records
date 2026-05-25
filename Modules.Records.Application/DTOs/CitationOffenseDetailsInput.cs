namespace Modules.Records.Application.DTOs;

/// <summary>
/// Jurisdiction-neutral offense detail for a citation save. The Texas-form-specific docket/page
/// fields are in <see cref="CitationTexasDetailsInput"/>, and the structured violation checkboxes
/// are carried separately on the command as a set of <see cref="Modules.Records.Domain.Common.Violations.ViolationFlagKey"/>.
/// </summary>
public sealed record CitationOffenseDetailsInput(
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
