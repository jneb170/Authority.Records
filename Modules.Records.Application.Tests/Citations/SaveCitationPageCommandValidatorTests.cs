using FluentValidation;
using Modules.Records.Application.Citations.Commands.SaveCitationPage;
using Modules.Records.Application.Citations.Validators;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Tests.Citations;

public sealed class SaveCitationPageCommandValidatorTests
{
    private readonly IValidator<SaveCitationPageCommand> _validator = new SaveCitationPageCommandValidator();

    [Fact]
    public void Validate_WithValidPayload_IsValid()
    {
        var command = new SaveCitationPageCommand(
            CitationId: Guid.NewGuid(),
            DefendantNameId: Guid.NewGuid(),
            Description: "Updated citation",
            IssueDate: DateTime.UtcNow.AddMinutes(-5),
            CourtId: null,
            CitationNum: "CT-100",
            AtTimeOfName: new NameSnapshotInput("Person", "Driver", "Jamie"),
            OfficerProfile: new CitationOfficerProfileInput(Guid.NewGuid(), 1234, "Officer Riley", "Officer", "B-42", "U-7"),
            TexasDetails: new CitationTexasDetailsInput("DKT-77", "2", Guid.NewGuid(), "545.351", Guid.NewGuid(), "Speeding", 72, 55, Guid.NewGuid(), "Unsafe speed", "IH-35", DateTime.UtcNow.AddDays(14), Guid.NewGuid(), DateTime.UtcNow.Date, "Officer Riley", "Jamie Driver", "Bond accepted", "RCPT-12"),
            Vehicle: new CitationVehicleInput("TX-ABC123", Guid.NewGuid(), 2025, 2022, "Ford", "SUV", "Blue", true, false),
            IncidentIdsToAdd: [Guid.NewGuid()],
            IncidentIdsToRemove: [Guid.NewGuid()],
            ChargeIdsToAdd: [Guid.NewGuid()],
            ChargeIdsToRemove: [Guid.NewGuid()]);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithFutureIssueDate_IsInvalid()
    {
        var command = new SaveCitationPageCommand(
            CitationId: Guid.NewGuid(),
            DefendantNameId: null,
            Description: "Updated citation",
            IssueDate: DateTime.UtcNow.AddMinutes(5),
            CourtId: null,
            CitationNum: "CT-100");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(command.IssueDate));
    }

    [Fact]
    public void Validate_WithAtTimeOfNameMissingLastName_IsInvalid()
    {
        var command = new SaveCitationPageCommand(
            CitationId: Guid.NewGuid(),
            DefendantNameId: Guid.NewGuid(),
            Description: "Updated citation",
            IssueDate: DateTime.UtcNow.AddMinutes(-5),
            CourtId: null,
            CitationNum: "CT-100",
            AtTimeOfName: new NameSnapshotInput("Person", string.Empty));

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "AtTimeOfName.LastOrBusinessName");
    }

    [Fact]
    public void Validate_WithOfficerProfileMissingOfficerName_IsInvalid()
    {
        var command = new SaveCitationPageCommand(
            CitationId: Guid.NewGuid(),
            DefendantNameId: Guid.NewGuid(),
            Description: "Updated citation",
            IssueDate: DateTime.UtcNow.AddMinutes(-5),
            CourtId: null,
            CitationNum: "CT-100",
            OfficerProfile: new CitationOfficerProfileInput(Guid.NewGuid(), 1234, string.Empty));

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "OfficerProfile.OfficerName");
    }

    [Fact]
    public void Validate_WithTexasDetailsOverlongDocket_IsInvalid()
    {
        var command = new SaveCitationPageCommand(
            CitationId: Guid.NewGuid(),
            DefendantNameId: Guid.NewGuid(),
            Description: "Updated citation",
            IssueDate: DateTime.UtcNow.AddMinutes(-5),
            CourtId: null,
            CitationNum: "CT-100",
            TexasDetails: new CitationTexasDetailsInput(new string('D', 51)));

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "TexasDetails.DocketNumber");
    }
}
