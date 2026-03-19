using FluentValidation;
using Modules.Records.Application.Arrests.Commands.SaveArrestPage;
using Modules.Records.Application.Arrests.Validators;

namespace Modules.Records.Application.Tests.Arrests;

public sealed class SaveArrestPageCommandValidatorTests
{
    private readonly IValidator<SaveArrestPageCommand> _validator = new SaveArrestPageCommandValidator();

    [Fact]
    public void Validate_WithValidPayload_IsValid()
    {
        var command = new SaveArrestPageCommand(
            ArrestId: Guid.NewGuid(),
            NameId: Guid.NewGuid(),
            ArrestedAt: DateTime.UtcNow.AddMinutes(-5),
            ArrestTypeId: null,
            ArrestNum: "AR-100",
            IncidentIdsToAdd: [Guid.NewGuid()],
            IncidentIdsToRemove: [Guid.NewGuid()],
            ChargeIdsToAdd: [Guid.NewGuid()],
            ChargeIdsToRemove: [Guid.NewGuid()]);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyNameId_IsInvalid()
    {
        var command = new SaveArrestPageCommand(
            ArrestId: Guid.NewGuid(),
            NameId: Guid.Empty,
            ArrestedAt: DateTime.UtcNow.AddMinutes(-5),
            ArrestTypeId: null,
            ArrestNum: "AR-100");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(command.NameId));
    }
}
