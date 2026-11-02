using Modules.Records.Domain.Common.Primitives;

namespace Modules.Records.Domain.DomainEvents;

/// <summary>
/// Raised when a name's details change. Deliberately carries no SSN: events are stored in the
/// outbox and copied into the audit log payload, so the value would end up in plain text in
/// both (issue #121). <see cref="SocialSecurityNumberChanged"/> records that it changed;
/// projections read the value from the Name itself.
/// </summary>
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
    string? PrimaryPhone,
    string? PrimaryPhoneExtension,
    string? WorkPhone,
    string? WorkPhoneExtension,
    string? OtherPhone,
    string? OtherPhoneExtension,
    bool SocialSecurityNumberChanged,
    bool IsCitizen,
    DateTime? DeceasedDate,
    Guid? PrimaryLocationId,
    Guid? SecondaryLocationId,
    Guid? ModifiedBy) : DomainEvent;
