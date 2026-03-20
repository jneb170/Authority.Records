using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Domain.Entities;

public sealed class CitationOfficerProfile : IMultiTenant
{
    public Guid Id { get; private set; }
    public Guid JurisdictionId { get; private set; }
    public Guid AgencyId { get; private set; }
    public Guid CitationId { get; private set; }
    public Guid? SourceNameId { get; private set; }
    public long? SourceNameRecordNumber { get; private set; }
    public string OfficerName { get; private set; } = string.Empty;
    public string? Title { get; private set; }
    public string? BadgeOrIdentifier { get; private set; }
    public string? UnitNumber { get; private set; }

    private CitationOfficerProfile()
    {
    }

    public CitationOfficerProfile(
        Guid jurisdictionId,
        Guid agencyId,
        Guid citationId,
        Guid? sourceNameId,
        long? sourceNameRecordNumber,
        string officerName,
        string? title,
        string? badgeOrIdentifier,
        string? unitNumber)
    {
        Id = Guid.NewGuid();
        JurisdictionId = jurisdictionId;
        AgencyId = agencyId;
        CitationId = citationId;
        UpdateDetails(sourceNameId, sourceNameRecordNumber, officerName, title, badgeOrIdentifier, unitNumber);
    }

    public void UpdateDetails(
        Guid? sourceNameId,
        long? sourceNameRecordNumber,
        string officerName,
        string? title,
        string? badgeOrIdentifier,
        string? unitNumber)
    {
        SourceNameId = sourceNameId;
        SourceNameRecordNumber = sourceNameRecordNumber;
        OfficerName = officerName;
        Title = title;
        BadgeOrIdentifier = badgeOrIdentifier;
        UnitNumber = unitNumber;
    }
}
