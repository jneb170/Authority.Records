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

        When(x => x.AtTimeOfName is not null, () =>
        {
            RuleFor(x => x.AtTimeOfName!.NameType)
                .NotEmpty().WithMessage("At Time Of name type is required.");

            RuleFor(x => x.AtTimeOfName!.LastOrBusinessName)
                .NotEmpty().WithMessage("At Time Of last or business name is required.");
        });

        When(x => x.OfficerProfile is not null, () =>
        {
            RuleFor(x => x.OfficerProfile!.OfficerName)
                .NotEmpty().WithMessage("Officer name is required when an officer profile is provided.")
                .MaximumLength(250).WithMessage("Officer name must not exceed 250 characters.");

            RuleFor(x => x.OfficerProfile!.Title)
                .MaximumLength(100).WithMessage("Officer title must not exceed 100 characters.");

            RuleFor(x => x.OfficerProfile!.BadgeOrIdentifier)
                .MaximumLength(50).WithMessage("Badge or identifier must not exceed 50 characters.");

            RuleFor(x => x.OfficerProfile!.UnitNumber)
                .MaximumLength(50).WithMessage("Unit number must not exceed 50 characters.");
        });

        When(x => x.Vehicle is not null, () =>
        {
            RuleFor(x => x.Vehicle!.PlateNumber)
                .MaximumLength(20).WithMessage("Plate number must not exceed 20 characters.");

            RuleFor(x => x.Vehicle!.Make)
                .MaximumLength(50).WithMessage("Make must not exceed 50 characters.");

            RuleFor(x => x.Vehicle!.Style)
                .MaximumLength(50).WithMessage("Style must not exceed 50 characters.");

            RuleFor(x => x.Vehicle!.Color)
                .MaximumLength(50).WithMessage("Color must not exceed 50 characters.");
        });

        When(x => x.TexasDetails is not null, () =>
        {
            RuleFor(x => x.TexasDetails!.DocketNumber)
                .MaximumLength(50).WithMessage("Docket number must not exceed 50 characters.");

            RuleFor(x => x.TexasDetails!.PageNumber)
                .MaximumLength(25).WithMessage("Page number must not exceed 25 characters.");

            RuleFor(x => x.TexasDetails!.ViolationSection)
                .MaximumLength(50).WithMessage("Violation section must not exceed 50 characters.");

            RuleFor(x => x.TexasDetails!.PrimaryViolationDescription)
                .MaximumLength(250).WithMessage("Primary violation description must not exceed 250 characters.");

            RuleFor(x => x.TexasDetails!.NarrativeOtherViolations)
                .MaximumLength(1000).WithMessage("Other violations text must not exceed 1000 characters.");

            RuleFor(x => x.TexasDetails!.OccurredAtText)
                .MaximumLength(250).WithMessage("Occurred-at text must not exceed 250 characters.");

            RuleFor(x => x.TexasDetails!.ComplainantSignatureText)
                .MaximumLength(150).WithMessage("Complainant signature text must not exceed 150 characters.");

            RuleFor(x => x.TexasDetails!.DefendantSignatureText)
                .MaximumLength(150).WithMessage("Defendant signature text must not exceed 150 characters.");

            RuleFor(x => x.TexasDetails!.AcceptedBondNotes)
                .MaximumLength(500).WithMessage("Bond notes must not exceed 500 characters.");

            RuleFor(x => x.TexasDetails!.ReceiptNumber)
                .MaximumLength(50).WithMessage("Receipt number must not exceed 50 characters.");

            RuleFor(x => x.TexasDetails!.SpeedMph)
                .GreaterThanOrEqualTo(0).When(x => x.TexasDetails!.SpeedMph.HasValue);

            RuleFor(x => x.TexasDetails!.ZoneMph)
                .GreaterThanOrEqualTo(0).When(x => x.TexasDetails!.ZoneMph.HasValue);
        });
    }
}
