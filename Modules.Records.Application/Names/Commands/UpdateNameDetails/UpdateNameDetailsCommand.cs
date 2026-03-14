using MediatR;

namespace Modules.Records.Application.Names.Commands.UpdateNameDetails;

public sealed record UpdateNameDetailsCommand(
    Guid NameId,
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
    Guid? PrimaryLocationId   = null,
    Guid? SecondaryLocationId = null) : IRequest;
