using FluentValidation;
using Modules.Records.Application.Charges.Commands.UpdateCharge;

namespace Modules.Records.Application.Tests.Charges;

public sealed class UpdateChargeCommandValidatorTests
{
    private readonly IValidator<UpdateChargeCommand> _validator = new UpdateChargeCommandValidator();

    [Fact]
    public void Validate_WithRequiredFields_IsValid()
    {
        var command = new UpdateChargeCommand(
            Guid.NewGuid(),
            "Disorderly Conduct",
            "Part II",
            "Group B",
            "Society",
            "90C",
            "Misdemeanor",
            "Class C",
            true,
            true);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithoutChargeId_IsInvalid()
    {
        var command = new UpdateChargeCommand(
            Guid.Empty,
            "Disorderly Conduct",
            "Part II",
            "Group B",
            "Society",
            "90C",
            "Misdemeanor",
            "Class C",
            true,
            true);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.ChargeId));
    }
}
