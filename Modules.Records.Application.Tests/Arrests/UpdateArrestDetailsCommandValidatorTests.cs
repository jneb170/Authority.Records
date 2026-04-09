using FluentValidation;
using Modules.Records.Application.Arrests.Commands.UpdateArrestDetails;
using Modules.Records.Application.Arrests.Validators;

namespace Modules.Records.Application.Tests.Arrests;

public sealed class UpdateArrestDetailsCommandValidatorTests
{
    private readonly IValidator<UpdateArrestDetailsCommand> _validator = new UpdateArrestDetailsCommandValidator();

    [Fact]
    public void Validate_WithNameIdAndPastArrestDate_IsValid()
    {
        var command = new UpdateArrestDetailsCommand(
            ArrestId: Guid.NewGuid(),
            NameId: Guid.NewGuid(),
            ArrestedAt: DateTime.UtcNow.AddHours(-1),
            ArrestTypeId: null);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyNameId_IsInvalid()
    {
        var command = new UpdateArrestDetailsCommand(
            ArrestId: Guid.NewGuid(),
            NameId: Guid.Empty,
            ArrestedAt: DateTime.UtcNow.AddHours(-1),
            ArrestTypeId: null);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.NameId));
    }

    [Fact]
    public void Validate_WithFutureArrestDate_IsInvalid()
    {
        var command = new UpdateArrestDetailsCommand(
            ArrestId: Guid.NewGuid(),
            NameId: Guid.NewGuid(),
            ArrestedAt: DateTime.UtcNow.AddMinutes(5),
            ArrestTypeId: null);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.ArrestedAt));
    }
}
