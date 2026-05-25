using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Domain.Entities;

/// <summary>
/// Texas-specific artifacts for a citation's printed Uniform Traffic Ticket (UTC) form. The
/// jurisdiction-neutral offense data (violation, speed, court appearance, signatures, etc.) lives in
/// <see cref="CitationOffenseDetails"/> for reuse across states, and the structured violation
/// checkboxes live in <see cref="CitationViolationFlag"/>; this entity holds only fields that are
/// peculiar to the Texas form layout. One row per citation.
/// </summary>
public sealed class CitationTexasDetails : IMultiTenant
{
    public Guid Id { get; private set; }
    public Guid JurisdictionId { get; private set; }
    public Guid AgencyId { get; private set; }
    public Guid CitationId { get; private set; }
    public string? DocketNumber { get; private set; }
    public string? PageNumber { get; private set; }

    private CitationTexasDetails()
    {
    }

    public CitationTexasDetails(
        Guid jurisdictionId,
        Guid agencyId,
        Guid citationId,
        string? docketNumber,
        string? pageNumber)
    {
        Id = Guid.NewGuid();
        JurisdictionId = jurisdictionId;
        AgencyId = agencyId;
        CitationId = citationId;
        UpdateDetails(docketNumber, pageNumber);
    }

    public void UpdateDetails(string? docketNumber, string? pageNumber)
    {
        DocketNumber = docketNumber;
        PageNumber = pageNumber;
    }
}
