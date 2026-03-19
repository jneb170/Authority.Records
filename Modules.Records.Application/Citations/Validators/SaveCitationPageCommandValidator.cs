using FluentValidation;
using Modules.Records.Application.Citations.Commands.SaveCitationPage;

namespace Modules.Records.Application.Citations.Validators;

public sealed class SaveCitationPageCommandValidator : AbstractValidator<SaveCitationPageCommand>
{
    public SaveCitationPageCommandValidator()
    {
        RuleFor(x => x.CitationId)
            .NotEmpty().WithMessage("Citation is required.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        RuleFor(x => x.IssueDate)
            .NotEmpty().WithMessage("Issue date is required.")
            .LessThanOrEqualTo(_ => DateTime.UtcNow).WithMessage("Issue date cannot be in the future.");

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
