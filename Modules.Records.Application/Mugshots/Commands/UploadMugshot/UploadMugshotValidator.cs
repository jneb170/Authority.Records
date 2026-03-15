using FluentValidation;
using Modules.Records.Domain.Common;

namespace Modules.Records.Application.Mugshots.Commands.UploadMugshot;

public sealed class UploadMugshotValidator : AbstractValidator<UploadMugshotCommand>
{
    private static readonly string[] SupportedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    public UploadMugshotValidator()
    {
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.OwnerType)
            .Must(MugshotOwnerTypes.IsSupported)
            .WithMessage("A supported mugshot owner type is required.");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.ContentType)
            .Must(contentType => SupportedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Only JPEG, PNG, and WebP mugshot uploads are supported.");

        RuleFor(x => x.Content)
            .NotNull()
            .Must(content => content.Length > 0)
            .WithMessage("Mugshot content is required.")
            .Must(content => content.Length <= 5 * 1024 * 1024)
            .WithMessage("Mugshot uploads must be 5 MB or smaller.");
    }
}
