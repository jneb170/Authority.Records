using FluentValidation;
using Modules.Records.Domain.Common;

namespace Modules.Records.Application.Mugshots.Commands.UnlinkMugshotFromOwner;

public sealed class UnlinkMugshotFromOwnerValidator : AbstractValidator<UnlinkMugshotFromOwnerCommand>
{
    public UnlinkMugshotFromOwnerValidator()
    {
        RuleFor(x => x.MugshotId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.OwnerType)
            .Must(MugshotOwnerTypes.IsSupported)
            .WithMessage("A supported mugshot owner type is required.");
    }
}
