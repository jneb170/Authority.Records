using Modules.Records.Application.DTOs;

namespace Modules.Records.UI.Services;

public interface INameService
{
    Task<NameDto?> GetByIdAsync(Guid id);
    Task<NameDto?> GetByRecordNumberAsync(long recordNumber);
    Task<IReadOnlyList<NameDto>> GetByJurisdictionAsync();
    Task<IReadOnlyList<NameDto>> SearchAsync(
        string? nameType             = null,
        string? nameContains         = null,
        Guid?   sexId                = null,
        Guid?   raceId               = null,
        DateTime? dateOfBirthFrom    = null,
        DateTime? dateOfBirthTo      = null,
        int?    heightInchesMin      = null,
        int?    heightInchesMax      = null,
        int?    weightLbsMin         = null,
        int?    weightLbsMax         = null,
        Guid?   hairColorId          = null,
        Guid?   eyeColorId           = null,
        string? driversLicenseNumber = null,
        Guid?   driversLicenseStateId = null);
    Task<long> CreateAsync(
        string nameType,
        string lastOrBusinessName,
        string? firstName,
        string? middleName,
        Guid? sexId,
        Guid? raceId,
        DateTime? dateOfBirth,
        string? driversLicenseNumber,
        Guid? driversLicenseStateId,
        int? heightInches,
        int? weightLbs,
        Guid? hairColorId,
        Guid? eyeColorId,
        Guid? suffixId = null,
        string? placeOfBirth = null,
        string? fbiNumber = null,
        string? localNumber = null,
        string? socialSecurityNumber = null,
        bool isCitizen = false,
        DateTime? deceasedDate = null,
        string? primaryPhone = null,
        string? primaryPhoneExtension = null,
        string? workPhone = null,
        string? workPhoneExtension = null,
        string? otherPhone = null,
        string? otherPhoneExtension = null);
    Task UpdateDetailsAsync(
        Guid id,
        string nameType,
        string lastOrBusinessName,
        string? firstName,
        string? middleName,
        Guid? sexId,
        Guid? raceId,
        DateTime? dateOfBirth,
        string? driversLicenseNumber,
        Guid? driversLicenseStateId,
        int? heightInches,
        int? weightLbs,
        Guid? hairColorId,
        Guid? eyeColorId,
        Guid? suffixId = null,
        string? placeOfBirth = null,
        string? fbiNumber = null,
        string? localNumber = null,
        string? socialSecurityNumber = null,
        bool isCitizen = false,
        DateTime? deceasedDate = null,
        Guid? primaryLocationId = null,
        Guid? secondaryLocationId = null,
        string? primaryPhone = null,
        string? primaryPhoneExtension = null,
        string? workPhone = null,
        string? workPhoneExtension = null,
        string? otherPhone = null,
        string? otherPhoneExtension = null);
    Task AcquireLockAsync(Guid id);
    Task RenewLockAsync(Guid id);
    Task ReleaseLockAsync(Guid id);
    Task SoftDeleteAsync(Guid id);
    Task RestoreAsync(Guid id);
}
