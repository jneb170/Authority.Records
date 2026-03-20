using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Domain.Entities;

public sealed class CitationTexasDetails : IMultiTenant
{
    public Guid Id { get; private set; }
    public Guid JurisdictionId { get; private set; }
    public Guid AgencyId { get; private set; }
    public Guid CitationId { get; private set; }
    public string? DocketNumber { get; private set; }
    public string? PageNumber { get; private set; }
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

    private CitationTexasDetails()
    {
    }

    public CitationTexasDetails(
        Guid jurisdictionId,
        Guid agencyId,
        Guid citationId,
        string? docketNumber,
        string? pageNumber,
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
            docketNumber,
            pageNumber,
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
        string? docketNumber,
        string? pageNumber,
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
        DocketNumber = docketNumber;
        PageNumber = pageNumber;
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
