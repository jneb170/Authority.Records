using FluentValidation;
using Modules.Records.Application.Incidents.Commands.SaveIncidentPage;
using Modules.Records.Application.Incidents.Validators;
using Modules.Records.Domain.ValueObjects;

namespace Modules.Records.Application.Tests.Incidents;

public sealed class SaveIncidentPageCommandValidatorTests
{
    private readonly IValidator<SaveIncidentPageCommand> _validator = new SaveIncidentPageCommandValidator();

    [Fact]
    public void Validate_WithValidPayload_IsValid()
    {
        var command = new SaveIncidentPageCommand(
            Guid.NewGuid(),
            new IncidentDetails
            {
                Description = "Updated incident",
                IncidentNum = "INC-100",
                LocalNum = "L-100",
                CFSNum = "CFS-100"
            },
            ChargeIdsToAdd: [Guid.NewGuid()],
            ChargeIdsToRemove: [Guid.NewGuid()]);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyIncidentId_IsInvalid()
    {
        var command = new SaveIncidentPageCommand(
            Guid.Empty,
            new IncidentDetails
            {
                Description = "Incident",
                IncidentNum = "INC-1",
                LocalNum = "LOC-1"
            });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(command.IncidentId));
    }
}
