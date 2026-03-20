namespace Modules.Records.Application.DTOs;

public sealed record CitationVehicleDto(
    string? PlateNumber = null,
    Guid? PlateStateId = null,
    int? PlateYear = null,
    int? ModelYear = null,
    string? Make = null,
    string? Style = null,
    string? Color = null,
    bool IsCommercial = false,
    bool CarriesHazardousMaterial = false);
