using FluentValidation;
using Modules.Records.Application.Arrests.Commands.SaveArrestPage;

namespace Modules.Records.Application.Arrests.Validators;

public sealed class SaveArrestPageCommandValidator : AbstractValidator<SaveArrestPageCommand>
{
    public SaveArrestPageCommandValidator()
    {
        RuleFor(x => x.ArrestId)
            .NotEmpty().WithMessage("Arrest is required.");

        RuleFor(x => x.NameId)
            .NotEmpty().WithMessage("Linked name is required.");

        RuleFor(x => x.ArrestedAt)
            .NotEmpty().WithMessage("Arrest date is required.")
            .LessThanOrEqualTo(_ => DateTime.UtcNow).WithMessage("Arrest date cannot be in the future.");

        RuleFor(x => x.IncidentIdsToAdd)
            .Must(ids => ids is null || ids.All(id => id != Guid.Empty))
            .WithMessage("Incident add operations must reference valid incidents.");

        RuleFor(x => x.IncidentIdsToRemove)
            .Must(ids => ids is null || ids.All(id => id != Guid.Empty))
            .WithMessage("Incident remove operations must reference valid incidents.");

        RuleFor(x => x.ChargeIdsToAdd)
            .Must(ids => ids is null || ids.All(id => id != Guid.Empty))
            .WithMessage("Charge add operations must reference valid charges.");

        RuleFor(x => x.ChargeIdsToRemove)
            .Must(ids => ids is null || ids.All(id => id != Guid.Empty))
            .WithMessage("Charge remove operations must reference valid charges.");
    }
}
