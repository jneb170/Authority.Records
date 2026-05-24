using MediatR;
using Modules.Records.Application.Common;

namespace Modules.Records.Application.Names.Commands.CreateName;

public sealed record CreateNameCommand(
    string NameType,
    string LastOrBusinessName,
    string? FirstName,
    string? MiddleName,
    Guid? SexId,
    Guid? RaceId,
    DateTime? DateOfBirth,
    string? DriversLicenseNumber,
    Guid? DriversLicenseStateId,
    int? HeightInches,
    int? WeightLbs,
    Guid? HairColorId,
    Guid? EyeColorId,
    Guid? SuffixId,
    string? PlaceOfBirth,
    string? FbiNumber,
    string? LocalNumber,
    string? SocialSecurityNumber,
    bool IsCitizen,
    DateTime? DeceasedDate,
    string? PrimaryPhone = null,
    string? PrimaryPhoneExtension = null,
    string? WorkPhone = null,
    string? WorkPhoneExtension = null,
    string? OtherPhone = null,
    string? OtherPhoneExtension = null) : IRequest<long>, IRateLimitedCommand;
