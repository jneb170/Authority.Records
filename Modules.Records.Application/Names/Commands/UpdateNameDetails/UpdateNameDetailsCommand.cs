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
    Guid? EyeColorId) : IRequest;
