using FluentValidation;
using Modules.Records.Application.Names.Commands.UpdateNameDetails;

namespace Modules.Records.Application.Names.Validators;

public sealed class UpdateNameDetailsCommandValidator : AbstractValidator<UpdateNameDetailsCommand>
{
    public UpdateNameDetailsCommandValidator()
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

        RuleFor(x => x.PrimaryPhone)
            .MaximumLength(25).WithMessage("Primary phone must not exceed 25 characters.")
            .When(x => x.PrimaryPhone != null);

        RuleFor(x => x.PrimaryPhoneExtension)
            .MaximumLength(10).WithMessage("Primary phone extension must not exceed 10 characters.")
            .When(x => x.PrimaryPhoneExtension != null);

        RuleFor(x => x.WorkPhone)
            .MaximumLength(25).WithMessage("Work phone must not exceed 25 characters.")
            .When(x => x.WorkPhone != null);

        RuleFor(x => x.WorkPhoneExtension)
            .MaximumLength(10).WithMessage("Work phone extension must not exceed 10 characters.")
            .When(x => x.WorkPhoneExtension != null);

        RuleFor(x => x.OtherPhone)
            .MaximumLength(25).WithMessage("Other phone must not exceed 25 characters.")
            .When(x => x.OtherPhone != null);

        RuleFor(x => x.OtherPhoneExtension)
            .MaximumLength(10).WithMessage("Other phone extension must not exceed 10 characters.")
            .When(x => x.OtherPhoneExtension != null);

        RuleFor(x => x.HeightInches)
            .InclusiveBetween(1, 120).WithMessage("Height must be between 1 and 120 inches.")
            .When(x => x.HeightInches.HasValue);

        RuleFor(x => x.WeightLbs)
            .InclusiveBetween(1, 1000).WithMessage("Weight must be between 1 and 1000 lbs.")
            .When(x => x.WeightLbs.HasValue);
    }
}
