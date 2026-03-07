using FluentValidation;
using Modules.Records.Application.Names.Commands.CreateName;

namespace Modules.Records.Application.Names.Validators;

public sealed class CreateNameCommandValidator : AbstractValidator<CreateNameCommand>
{
    public CreateNameCommandValidator()
    {
        RuleFor(x => x.LastOrBusinessName)
            .NotEmpty().WithMessage("Last name (or business name) is required.")
            .MaximumLength(250).WithMessage("Last/Business name must not exceed 250 characters.");

        RuleFor(x => x.FirstName)
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.")
            .When(x => x.FirstName != null);

        RuleFor(x => x.MiddleName)
            .MaximumLength(100).WithMessage("Middle name must not exceed 100 characters.")
            .When(x => x.MiddleName != null);

        RuleFor(x => x.DriversLicenseNumber)
            .MaximumLength(50).WithMessage("Driver's license number must not exceed 50 characters.")
            .When(x => x.DriversLicenseNumber != null);

        RuleFor(x => x.HeightInches)
            .InclusiveBetween(1, 120).WithMessage("Height must be between 1 and 120 inches.")
            .When(x => x.HeightInches.HasValue);

        RuleFor(x => x.WeightLbs)
            .InclusiveBetween(1, 1000).WithMessage("Weight must be between 1 and 1000 lbs.")
            .When(x => x.WeightLbs.HasValue);
    }
}
