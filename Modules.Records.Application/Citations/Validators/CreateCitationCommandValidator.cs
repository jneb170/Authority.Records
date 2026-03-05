using FluentValidation;
using Modules.Records.Application.Citations.Commands.CreateCitation;

namespace Modules.Records.Application.Citations.Validators;

public sealed class CreateCitationCommandValidator : AbstractValidator<CreateCitationCommand>
{
    public CreateCitationCommandValidator()
    {
        RuleFor(x => x.IncidentId)
            .NotEmpty().WithMessage("IncidentId is required.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        RuleFor(x => x.IssueDate)
            .NotEmpty().WithMessage("Issue date is required.")
            .LessThanOrEqualTo(_ => DateTime.UtcNow).WithMessage("Issue date cannot be in the future.");
    }
}
