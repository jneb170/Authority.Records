using FluentValidation;
using Modules.Records.Application.Charges.Commands.CreateCharge;

namespace Modules.Records.Application.Tests.Charges;

public sealed class CreateChargeCommandValidatorTests
{
    private readonly IValidator<CreateChargeCommand> _validator = new CreateChargeCommandValidator();

    [Fact]
    public void Validate_WithRequiredFields_IsValid()
    {
        var command = new CreateChargeCommand(
            "Public Intoxication",
            "Part II",
            "Group B",
            "Society",
            "90E",
            "Misdemeanor",
            "Class C",
            true);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithoutOffenseName_IsInvalid()
    {
        var command = new CreateChargeCommand(
            string.Empty,
            "Part II",
            "Group B",
            "Society",
            "90E",
            "Misdemeanor",
            "Class C",
            true);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.OffenseName));
    }
}
