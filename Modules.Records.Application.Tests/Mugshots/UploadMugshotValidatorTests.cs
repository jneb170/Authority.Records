using FluentValidation;
using Modules.Records.Application.Mugshots.Commands.UploadMugshot;
using Modules.Records.Domain.Common;

namespace Modules.Records.Application.Tests.Mugshots;

public sealed class UploadMugshotValidatorTests
{
    private readonly IValidator<UploadMugshotCommand> _validator = new UploadMugshotValidator();

    [Fact]
    public void Validate_WithSupportedImageUpload_IsValid()
    {
        var command = new UploadMugshotCommand(
            MugshotOwnerTypes.Name,
            Guid.NewGuid(),
            "booking-photo.jpg",
            "image/jpeg",
            [1, 2, 3, 4],
            MakePrimary: true);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithUnsupportedOwnerType_IsInvalid()
    {
        var command = new UploadMugshotCommand(
            "Incident",
            Guid.NewGuid(),
            "booking-photo.jpg",
            "image/jpeg",
            [1, 2, 3, 4]);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.OwnerType));
    }

    [Fact]
    public void Validate_WithUnsupportedContentType_IsInvalid()
    {
        var command = new UploadMugshotCommand(
            MugshotOwnerTypes.Arrest,
            Guid.NewGuid(),
            "booking-photo.gif",
            "image/gif",
            [1, 2, 3, 4]);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.ContentType));
    }

    [Fact]
    public void Validate_WithOversizedPayload_IsInvalid()
    {
        var content = new byte[5 * 1024 * 1024 + 1];
        var command = new UploadMugshotCommand(
            MugshotOwnerTypes.Name,
            Guid.NewGuid(),
            "booking-photo.png",
            "image/png",
            content);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Content));
    }
}
