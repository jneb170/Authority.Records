using FluentValidation;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Narratives.Commands.UpdateNarrativeContent;

public sealed class UpdateNarrativeContentValidator : AbstractValidator<UpdateNarrativeContentCommand>
{
    public UpdateNarrativeContentValidator()
    {
        RuleFor(x => x.NarrativeId).NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Narrative title is required.")
            .MaximumLength(Narrative.MaxTitleLength)
            .WithMessage($"Narrative title must not exceed {Narrative.MaxTitleLength} characters.");

        RuleFor(x => x.Content)
            .MaximumLength(Narrative.MaxContentLength)
            .WithMessage($"Narrative content must not exceed {Narrative.MaxContentLength:N0} characters.");
    }
}
