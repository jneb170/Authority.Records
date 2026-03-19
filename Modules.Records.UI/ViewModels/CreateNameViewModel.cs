using Modules.Records.Domain.Common;

namespace Modules.Records.UI.ViewModels;

public sealed class CreateNameViewModel
{
    public string NameType { get; set; } = NameTypes.Person;
    public string LastOrBusinessName { get; set; } = string.Empty;

    // Person-only fields
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public Guid? SuffixId { get; set; }
    public Guid? SexId { get; set; }
    public Guid? RaceId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? PlaceOfBirth { get; set; }
    public string? DriversLicenseNumber { get; set; }
    public Guid? DriversLicenseStateId { get; set; }
    public string? PrimaryPhone { get; set; }
    public string? PrimaryPhoneExtension { get; set; }
    public string? WorkPhone { get; set; }
    public string? WorkPhoneExtension { get; set; }
    public string? OtherPhone { get; set; }
    public string? OtherPhoneExtension { get; set; }
    public int? HeightFeet { get; set; }
    public int? HeightInchesRemainder { get; set; }
    public int? WeightLbs { get; set; }
    public Guid? HairColorId { get; set; }
    public Guid? EyeColorId { get; set; }
    public string? SocialSecurityNumber { get; set; }
    public string? FbiNumber { get; set; }
    public string? LocalNumber { get; set; }
    public bool IsCitizen { get; set; }
    public DateTime? DeceasedDate { get; set; }

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
        SuffixId = null;
        SexId = null;
        RaceId = null;
        DateOfBirth = null;
        PlaceOfBirth = null;
        DriversLicenseNumber = null;
        DriversLicenseStateId = null;
        PrimaryPhone = null;
        PrimaryPhoneExtension = null;
        WorkPhone = null;
        WorkPhoneExtension = null;
        OtherPhone = null;
        OtherPhoneExtension = null;
        HeightFeet = null;
        HeightInchesRemainder = null;
        WeightLbs = null;
        HairColorId = null;
        EyeColorId = null;
        SocialSecurityNumber = null;
        FbiNumber = null;
        LocalNumber = null;
        IsCitizen = false;
        DeceasedDate = null;
        ErrorMessage = null;
    }
}
