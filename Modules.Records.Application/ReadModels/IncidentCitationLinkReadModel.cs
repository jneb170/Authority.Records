namespace Modules.Records.Application.ReadModels;

public sealed class IncidentCitationLinkReadModel
{
    public Guid Id { get; private set; }
    public Guid JurisdictionId { get; private set; }
    public Guid IncidentId { get; private set; }
    public long IncidentRecordNumber { get; private set; }
    public string IncidentNum { get; private set; } = string.Empty;
    public Guid CitationId { get; private set; }
    public DateTime LinkedAtUtc { get; private set; }

    private IncidentCitationLinkReadModel() { } // EF

    public static IncidentCitationLinkReadModel Create(
        Guid id,
        Guid jurisdictionId,
        Guid incidentId,
        long incidentRecordNumber,
        string incidentNum,
        Guid citationId,
        DateTime linkedAtUtc)
    {
        return new IncidentCitationLinkReadModel
        {
            Id = id,
            JurisdictionId = jurisdictionId,
            IncidentId = incidentId,
            IncidentRecordNumber = incidentRecordNumber,
            IncidentNum = incidentNum,
            CitationId = citationId,
            LinkedAtUtc = linkedAtUtc
        };
    }
}
