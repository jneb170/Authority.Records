using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Domain.Entities;

/// <summary>
/// Jurisdiction-neutral offense detail for a citation: the common scalar fields any state's citation
/// form would use (violation classification, speed, court appearance, signatures, bond, receipt).
/// State-form-specific artifacts live elsewhere — see <see cref="CitationTexasDetails"/> — and the
/// structured violation checkboxes live in <see cref="CitationViolationFlag"/>. One row per citation.
/// Like the other supplemental entities this is a plain <see cref="IMultiTenant"/> entity scoped by
/// <see cref="CitationId"/>, cascade-deleted with its citation.
/// </summary>
public sealed class CitationOffenseDetails : IMultiTenant
{
    public Guid Id { get; private set; }
    public Guid JurisdictionId { get; private set; }
    public Guid AgencyId { get; private set; }
    public Guid CitationId { get; private set; }
    public Guid? ViolationSourceTypeId { get; private set; }
    public string? ViolationSection { get; private set; }
    public Guid? ViolationGroupId { get; private set; }
    public string? PrimaryViolationDescription { get; private set; }
    public int? SpeedMph { get; private set; }
    public int? ZoneMph { get; private set; }
    public Guid? SpeedBandId { get; private set; }
    public string? NarrativeOtherViolations { get; private set; }
    public string? OccurredAtText { get; private set; }
    public DateTime? CourtAppearanceDateTime { get; private set; }
    public Guid? CourtAppearanceLocationId { get; private set; }
    public DateTime? AffidavitSignedDate { get; private set; }
    public string? ComplainantSignatureText { get; private set; }
    public string? DefendantSignatureText { get; private set; }
    public string? AcceptedBondNotes { get; private set; }
    public string? ReceiptNumber { get; private set; }

    private CitationOffenseDetails()
    {
    }

    public CitationOffenseDetails(
        Guid jurisdictionId,
        Guid agencyId,
        Guid citationId,
        Guid? violationSourceTypeId,
        string? violationSection,
        Guid? violationGroupId,
        string? primaryViolationDescription,
        int? speedMph,
        int? zoneMph,
        Guid? speedBandId,
        string? narrativeOtherViolations,
        string? occurredAtText,
        DateTime? courtAppearanceDateTime,
        Guid? courtAppearanceLocationId,
        DateTime? affidavitSignedDate,
        string? complainantSignatureText,
        string? defendantSignatureText,
        string? acceptedBondNotes,
        string? receiptNumber)
    {
        Id = Guid.NewGuid();
        JurisdictionId = jurisdictionId;
        AgencyId = agencyId;
        CitationId = citationId;
        UpdateDetails(
            violationSourceTypeId,
            violationSection,
            violationGroupId,
            primaryViolationDescription,
            speedMph,
            zoneMph,
            speedBandId,
            narrativeOtherViolations,
            occurredAtText,
            courtAppearanceDateTime,
            courtAppearanceLocationId,
            affidavitSignedDate,
            complainantSignatureText,
            defendantSignatureText,
            acceptedBondNotes,
            receiptNumber);
    }

    public void UpdateDetails(
        Guid? violationSourceTypeId,
        string? violationSection,
        Guid? violationGroupId,
        string? primaryViolationDescription,
        int? speedMph,
        int? zoneMph,
        Guid? speedBandId,
        string? narrativeOtherViolations,
        string? occurredAtText,
        DateTime? courtAppearanceDateTime,
        Guid? courtAppearanceLocationId,
        DateTime? affidavitSignedDate,
        string? complainantSignatureText,
        string? defendantSignatureText,
        string? acceptedBondNotes,
        string? receiptNumber)
    {
        ViolationSourceTypeId = violationSourceTypeId;
        ViolationSection = violationSection;
        ViolationGroupId = violationGroupId;
        PrimaryViolationDescription = primaryViolationDescription;
        SpeedMph = speedMph;
        ZoneMph = zoneMph;
        SpeedBandId = speedBandId;
        NarrativeOtherViolations = narrativeOtherViolations;
        OccurredAtText = occurredAtText;
        CourtAppearanceDateTime = courtAppearanceDateTime;
        CourtAppearanceLocationId = courtAppearanceLocationId;
        AffidavitSignedDate = affidavitSignedDate;
        ComplainantSignatureText = complainantSignatureText;
        DefendantSignatureText = defendantSignatureText;
        AcceptedBondNotes = acceptedBondNotes;
        ReceiptNumber = receiptNumber;
    }
}
