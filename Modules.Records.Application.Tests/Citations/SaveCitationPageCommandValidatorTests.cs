using FluentValidation;
using Modules.Records.Application.Citations.Commands.SaveCitationPage;
using Modules.Records.Application.Citations.Validators;

namespace Modules.Records.Application.Tests.Citations;

public sealed class SaveCitationPageCommandValidatorTests
{
    private readonly IValidator<SaveCitationPageCommand> _validator = new SaveCitationPageCommandValidator();

    [Fact]
    public void Validate_WithValidPayload_IsValid()
    {
        var command = new SaveCitationPageCommand(
            CitationId: Guid.NewGuid(),
            Description: "Updated citation",
            IssueDate: DateTime.UtcNow.AddMinutes(-5),
            CourtId: null,
            CitationNum: "CT-100",
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
            Description: "Updated citation",
            IssueDate: DateTime.UtcNow.AddMinutes(5),
            CourtId: null,
            CitationNum: "CT-100");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(command.IssueDate));
    }
}
