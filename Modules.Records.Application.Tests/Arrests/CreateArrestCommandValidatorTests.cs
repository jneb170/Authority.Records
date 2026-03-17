using FluentValidation;
using Modules.Records.Application.Arrests.Commands.CreateArrest;
using Modules.Records.Application.Arrests.Validators;

namespace Modules.Records.Application.Tests.Arrests;

public sealed class CreateArrestCommandValidatorTests
{
    private readonly IValidator<CreateArrestCommand> _validator = new CreateArrestCommandValidator();

    [Fact]
    public void Validate_WithNameIdAndPastArrestDate_IsValid()
    {
        var command = new CreateArrestCommand(
            NameId: Guid.NewGuid(),
            ArrestedAt: DateTime.UtcNow.AddHours(-1),
            IncidentRecordNumbers: []);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithoutNameId_IsInvalid()
    {
        var command = new CreateArrestCommand(
            NameId: null,
            ArrestedAt: DateTime.UtcNow.AddHours(-1),
            IncidentRecordNumbers: []);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.NameId));
    }

    [Fact]
    public void Validate_WithFutureArrestDate_IsInvalid()
    {
        var command = new CreateArrestCommand(
            NameId: Guid.NewGuid(),
            ArrestedAt: DateTime.UtcNow.AddMinutes(5),
            IncidentRecordNumbers: []);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.ArrestedAt));
    }
}
