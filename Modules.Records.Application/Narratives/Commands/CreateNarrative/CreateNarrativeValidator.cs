using FluentValidation;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Narratives.Commands.CreateNarrative;

public sealed class CreateNarrativeValidator : AbstractValidator<CreateNarrativeCommand>
{
    public CreateNarrativeValidator()
    {
        RuleFor(x => x.OwnerType)
            .Must(NarrativeOwnerTypes.IsSupported)
            .WithMessage("A supported narrative owner type is required.");

        RuleFor(x => x.OwnerId).NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Narrative title is required.")
            .MaximumLength(Narrative.MaxTitleLength)
            .WithMessage($"Narrative title must not exceed {Narrative.MaxTitleLength} characters.");

        RuleFor(x => x.Content)
            .MaximumLength(Narrative.MaxContentLength)
            .WithMessage($"Narrative content must not exceed {Narrative.MaxContentLength:N0} characters.");
    }
}
