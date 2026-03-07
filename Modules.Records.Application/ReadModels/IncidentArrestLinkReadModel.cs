namespace Modules.Records.Application.ReadModels;

public sealed class IncidentArrestLinkReadModel
{
    public Guid Id { get; private set; }
    public Guid JurisdictionId { get; private set; }
    public Guid IncidentId { get; private set; }
    public long IncidentRecordNumber { get; private set; }
    public string IncidentNum { get; private set; } = string.Empty;
    public Guid ArrestId { get; private set; }
    public DateTime LinkedAtUtc { get; private set; }

    private IncidentArrestLinkReadModel() { } // EF

    public static IncidentArrestLinkReadModel Create(
        Guid id,
        Guid jurisdictionId,
        Guid incidentId,
        long incidentRecordNumber,
        string incidentNum,
        Guid arrestId,
        DateTime linkedAtUtc)
    {
        return new IncidentArrestLinkReadModel
        {
            Id = id,
            JurisdictionId = jurisdictionId,
            IncidentId = incidentId,
            IncidentRecordNumber = incidentRecordNumber,
            IncidentNum = incidentNum,
            ArrestId = arrestId,
            LinkedAtUtc = linkedAtUtc
        };
    }
}
