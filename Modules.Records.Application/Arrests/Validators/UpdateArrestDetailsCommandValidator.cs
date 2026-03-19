using FluentValidation;
using Modules.Records.Application.Arrests.Commands.UpdateArrestDetails;

namespace Modules.Records.Application.Arrests.Validators;

public sealed class UpdateArrestDetailsCommandValidator : AbstractValidator<UpdateArrestDetailsCommand>
{
    public UpdateArrestDetailsCommandValidator()
    {
        RuleFor(x => x.NameId)
            .NotEmpty().WithMessage("Linked name is required.");

        RuleFor(x => x.ArrestedAt)
            .NotEmpty().WithMessage("Arrest date is required.")
            .LessThanOrEqualTo(_ => DateTime.UtcNow).WithMessage("Arrest date cannot be in the future.");

        When(x => x.AtTimeOfName is not null, () =>
        {
            RuleFor(x => x.AtTimeOfName!.NameType)
                .NotEmpty().WithMessage("At Time Of name type is required.");

            RuleFor(x => x.AtTimeOfName!.LastOrBusinessName)
                .NotEmpty().WithMessage("At Time Of last or business name is required.");
        });
    }
}
