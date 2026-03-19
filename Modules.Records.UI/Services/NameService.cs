using MediatR;
using Modules.Records.Application.DTOs;
using Modules.Records.Application.Names.Commands.AcquireNameLock;
using Modules.Records.Application.Names.Commands.CreateName;
using Modules.Records.Application.Names.Commands.ReleaseNameLock;
using Modules.Records.Application.Names.Commands.RestoreName;
using Modules.Records.Application.Names.Commands.SoftDeleteName;
using Modules.Records.Application.Names.Commands.UpdateNameDetails;
using Modules.Records.Application.Names.Queries.GetNameById;
using Modules.Records.Application.Names.Queries.GetNameByRecordNumber;
using Modules.Records.Application.Names.Queries.GetNamesByJurisdiction;
using Modules.Records.Application.Names.Queries.SearchNames;

namespace Modules.Records.UI.Services;

public sealed class NameService : INameService
{
    private readonly ISender _sender;

    public NameService(ISender sender) => _sender = sender;

    public Task<NameDto?> GetByIdAsync(Guid id) =>
        _sender.Send(new GetNameByIdQuery(id));

    public Task<NameDto?> GetByRecordNumberAsync(long recordNumber) =>
        _sender.Send(new GetNameByRecordNumberQuery(recordNumber));

    public Task<IReadOnlyList<NameDto>> GetByJurisdictionAsync() =>
        _sender.Send(new GetNamesByJurisdictionQuery());

    public Task<IReadOnlyList<NameDto>> SearchAsync(
        string? nameType              = null,
        string? nameContains          = null,
        Guid?   sexId                 = null,
        Guid?   raceId                = null,
        DateTime? dateOfBirthFrom     = null,
        DateTime? dateOfBirthTo       = null,
        int?    heightInchesMin       = null,
        int?    heightInchesMax       = null,
        int?    weightLbsMin          = null,
        int?    weightLbsMax          = null,
        Guid?   hairColorId           = null,
        Guid?   eyeColorId            = null,
        string? driversLicenseNumber  = null,
        Guid?   driversLicenseStateId = null) =>
        _sender.Send(new SearchNamesQuery(
            nameType, nameContains, sexId, raceId,
            dateOfBirthFrom, dateOfBirthTo,
            heightInchesMin, heightInchesMax,
            weightLbsMin, weightLbsMax,
            hairColorId, eyeColorId,
            driversLicenseNumber, driversLicenseStateId));

    public Task<long> CreateAsync(
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
        string? otherPhoneExtension = null) =>
        _sender.Send(new CreateNameCommand(
            nameType, lastOrBusinessName, firstName, middleName,
            sexId, raceId, dateOfBirth,
            driversLicenseNumber, driversLicenseStateId,
            heightInches, weightLbs, hairColorId, eyeColorId,
            suffixId, placeOfBirth, fbiNumber, localNumber,
            socialSecurityNumber, isCitizen, deceasedDate,
            primaryPhone, primaryPhoneExtension, workPhone, workPhoneExtension, otherPhone, otherPhoneExtension));

    public Task UpdateDetailsAsync(
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
        string? otherPhoneExtension = null) =>
        _sender.Send(new UpdateNameDetailsCommand(
            id, nameType, lastOrBusinessName, firstName, middleName,
            sexId, raceId, dateOfBirth,
            driversLicenseNumber, driversLicenseStateId,
            heightInches, weightLbs, hairColorId, eyeColorId,
            suffixId, placeOfBirth, fbiNumber, localNumber,
            socialSecurityNumber, isCitizen, deceasedDate,
            primaryLocationId, secondaryLocationId,
            primaryPhone, primaryPhoneExtension, workPhone, workPhoneExtension, otherPhone, otherPhoneExtension));

    public Task AcquireLockAsync(Guid id) =>
        _sender.Send(new AcquireNameLockCommand(id));

    public Task ReleaseLockAsync(Guid id) =>
        _sender.Send(new ReleaseNameLockCommand(id));

    public Task SoftDeleteAsync(Guid id) =>
        _sender.Send(new SoftDeleteNameCommand(id));

    public Task RestoreAsync(Guid id) =>
        _sender.Send(new RestoreNameCommand(id));
}
