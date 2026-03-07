using FluentValidation;
using Modules.Records.Application.Incidents.Commands.CreateIncident;

namespace Modules.Records.Application.Incidents.Validators;

public sealed class CreateIncidentCommandValidator : AbstractValidator<CreateIncidentCommand>
{
    public CreateIncidentCommandValidator()
    {
        RuleFor(x => x.AgencyId)
            .NotEmpty().WithMessage("AgencyId is required.");

        RuleFor(x => x.Details.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        RuleFor(x => x.Details.CFSNum)
            .MaximumLength(30).WithMessage("CFSNum must not exceed 30 characters.");
    }
}
