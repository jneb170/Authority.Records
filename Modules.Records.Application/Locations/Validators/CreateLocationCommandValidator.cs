using FluentValidation;
using Modules.Records.Application.Locations.Commands.CreateLocation;

namespace Modules.Records.Application.Locations.Validators;

public sealed class CreateLocationCommandValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationCommandValidator()
    {
        RuleFor(x => x.StreetAddress)
            .NotEmpty().WithMessage("Street address is required.")
            .MaximumLength(200).WithMessage("Street address must not exceed 200 characters.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100).WithMessage("City must not exceed 100 characters.");

        RuleFor(x => x.StreetNumber)
            .MaximumLength(20).WithMessage("Street number must not exceed 20 characters.")
            .When(x => x.StreetNumber != null);

        RuleFor(x => x.Zip)
            .MaximumLength(10).WithMessage("Zip code must not exceed 10 characters.")
            .When(x => x.Zip != null);

        RuleFor(x => x.AptSuite)
            .MaximumLength(50).WithMessage("Apt/Suite must not exceed 50 characters.")
            .When(x => x.AptSuite != null);

        RuleFor(x => x.CommonPlaceName)
            .MaximumLength(250).WithMessage("Common place name must not exceed 250 characters.")
            .When(x => x.CommonPlaceName != null);

        RuleFor(x => x.Comments)
            .MaximumLength(500).WithMessage("Comments must not exceed 500 characters.")
            .When(x => x.Comments != null);
    }
}
