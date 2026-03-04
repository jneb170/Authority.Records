namespace Modules.Records.Domain.Common;

public sealed class JurisdictionConfiguration
{
    public Guid Id { get; private set; }

    public Guid JurisdictionId { get; private set; }

    public bool MustCloseAllArrests { get; }

    public bool MustCloseAllCitations { get; }

    public bool MustCloseArrestsBeforeIncidentClose { get; private set; }

    private JurisdictionConfiguration() { }

    public JurisdictionConfiguration(Guid jurisdictionId, bool mustCloseAllArrests, bool mustCloseAllCitations)
    {
        Id = Guid.NewGuid();
        JurisdictionId = jurisdictionId;
        MustCloseAllArrests = mustCloseAllArrests;
        MustCloseAllCitations = mustCloseAllCitations;
    }
}