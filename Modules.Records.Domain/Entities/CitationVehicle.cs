using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Domain.Entities;

public sealed class CitationVehicle : IMultiTenant
{
    public Guid Id { get; private set; }
    public Guid JurisdictionId { get; private set; }
    public Guid AgencyId { get; private set; }
    public Guid CitationId { get; private set; }
    public string? PlateNumber { get; private set; }
    public Guid? PlateStateId { get; private set; }
    public int? PlateYear { get; private set; }
    public int? ModelYear { get; private set; }
    public string? Make { get; private set; }
    public string? Style { get; private set; }
    public string? Color { get; private set; }
    public bool IsCommercial { get; private set; }
    public bool CarriesHazardousMaterial { get; private set; }

    private CitationVehicle()
    {
    }

    public CitationVehicle(
        Guid jurisdictionId,
        Guid agencyId,
        Guid citationId,
        string? plateNumber,
        Guid? plateStateId,
        int? plateYear,
        int? modelYear,
        string? make,
        string? style,
        string? color,
        bool isCommercial,
        bool carriesHazardousMaterial)
    {
        Id = Guid.NewGuid();
        JurisdictionId = jurisdictionId;
        AgencyId = agencyId;
        CitationId = citationId;
        UpdateDetails(plateNumber, plateStateId, plateYear, modelYear, make, style, color, isCommercial, carriesHazardousMaterial);
    }

    public void UpdateDetails(
        string? plateNumber,
        Guid? plateStateId,
        int? plateYear,
        int? modelYear,
        string? make,
        string? style,
        string? color,
        bool isCommercial,
        bool carriesHazardousMaterial)
    {
        PlateNumber = plateNumber;
        PlateStateId = plateStateId;
        PlateYear = plateYear;
        ModelYear = modelYear;
        Make = make;
        Style = style;
        Color = color;
        IsCommercial = isCommercial;
        CarriesHazardousMaterial = carriesHazardousMaterial;
    }
}
