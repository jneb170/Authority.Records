using Modules.Records.Domain.Common.Primitives;

namespace Modules.Records.Domain.DomainEvents;

public sealed record NameDetailsUpdatedDomainEvent(
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
    DateTime? DeceasedDate) : DomainEvent;
