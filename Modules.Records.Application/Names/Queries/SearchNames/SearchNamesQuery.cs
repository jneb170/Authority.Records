using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Names.Queries.SearchNames;

/// <summary>
/// Flexible MNI search. All filter parameters are optional — provide any combination.
/// Name search uses case-insensitive contains on LastOrBusinessName, FirstName, and MiddleName.
/// </summary>
public sealed record SearchNamesQuery(
    string? NameType             = null,
    string? NameContains         = null,
    Guid?   SexId                = null,
    Guid?   RaceId               = null,
    DateTime? DateOfBirthFrom    = null,
    DateTime? DateOfBirthTo      = null,
    int?    HeightInchesMin      = null,
    int?    HeightInchesMax      = null,
    int?    WeightLbsMin         = null,
    int?    WeightLbsMax         = null,
    Guid?   HairColorId          = null,
    Guid?   EyeColorId           = null,
    string? DriversLicenseNumber = null,
    Guid?   DriversLicenseStateId = null)
    : IRequest<IReadOnlyList<NameDto>>;
