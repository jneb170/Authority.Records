using FluentValidation;
using Modules.Records.Application.Arrests.Commands.CreateArrest;

namespace Modules.Records.Application.Arrests.Validators;

public sealed class CreateArrestCommandValidator : AbstractValidator<CreateArrestCommand>
{
    public CreateArrestCommandValidator()
    {
        RuleFor(x => x.SuspectName)
            .MaximumLength(250).WithMessage("Suspect name must not exceed 250 characters.");

        RuleFor(x => x.ArrestedAt)
            .NotEmpty().WithMessage("Arrest date is required.")
            .LessThanOrEqualTo(_ => DateTime.UtcNow).WithMessage("Arrest date cannot be in the future.");
    }
}
