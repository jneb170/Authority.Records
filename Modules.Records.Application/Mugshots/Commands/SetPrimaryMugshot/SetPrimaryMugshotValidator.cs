using FluentValidation;
using Modules.Records.Domain.Common;

namespace Modules.Records.Application.Mugshots.Commands.SetPrimaryMugshot;

public sealed class SetPrimaryMugshotValidator : AbstractValidator<SetPrimaryMugshotCommand>
{
    public SetPrimaryMugshotValidator()
    {
        RuleFor(x => x.MugshotId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.OwnerType)
            .Must(MugshotOwnerTypes.IsSupported)
            .WithMessage("A supported mugshot owner type is required.");
    }
}
