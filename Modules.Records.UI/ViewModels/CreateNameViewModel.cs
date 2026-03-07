using Modules.Records.Domain.Common;

namespace Modules.Records.UI.ViewModels;

public sealed class CreateNameViewModel
{
    public string NameType { get; set; } = NameTypes.Person;
    public string LastOrBusinessName { get; set; } = string.Empty;

    // Person-only fields
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public Guid? SexId { get; set; }
    public Guid? RaceId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? DriversLicenseNumber { get; set; }
    public Guid? DriversLicenseStateId { get; set; }
    public int? HeightFeet { get; set; }
    public int? HeightInchesRemainder { get; set; }
    public int? WeightLbs { get; set; }
    public Guid? HairColorId { get; set; }
    public Guid? EyeColorId { get; set; }

    public string? ErrorMessage { get; set; }

    public bool IsPerson => NameType == NameTypes.Person;

    /// <summary>Converts ft/in UI inputs to total inches for storage.</summary>
    public int? HeightInches =>
        (HeightFeet.HasValue || HeightInchesRemainder.HasValue)
            ? (HeightFeet ?? 0) * 12 + (HeightInchesRemainder ?? 0)
            : null;

    public void Reset()
    {
        NameType = NameTypes.Person;
        LastOrBusinessName = string.Empty;
        FirstName = null;
        MiddleName = null;
        SexId = null;
        RaceId = null;
        DateOfBirth = null;
        DriversLicenseNumber = null;
        DriversLicenseStateId = null;
        HeightFeet = null;
        HeightInchesRemainder = null;
        WeightLbs = null;
        HairColorId = null;
        EyeColorId = null;
        ErrorMessage = null;
    }
}
