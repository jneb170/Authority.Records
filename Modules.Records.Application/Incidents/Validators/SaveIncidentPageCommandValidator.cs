using FluentValidation;
using Modules.Records.Application.Incidents.Commands.SaveIncidentPage;

namespace Modules.Records.Application.Incidents.Validators;

public sealed class SaveIncidentPageCommandValidator : AbstractValidator<SaveIncidentPageCommand>
{
    public SaveIncidentPageCommandValidator()
    {
        RuleFor(x => x.IncidentId)
            .NotEmpty().WithMessage("Incident is required.");

        RuleFor(x => x.Details)
            .NotNull().WithMessage("Incident details are required.");

        RuleFor(x => x.ChargeIdsToAdd)
            .Must(ids => ids is null || ids.All(id => id != Guid.Empty))
            .WithMessage("Charge add operations must reference valid charges.");

        RuleFor(x => x.ChargeIdsToRemove)
            .Must(ids => ids is null || ids.All(id => id != Guid.Empty))
            .WithMessage("Charge remove operations must reference valid charges.");
    }
}
