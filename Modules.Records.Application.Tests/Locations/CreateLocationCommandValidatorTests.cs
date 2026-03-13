using FluentValidation;
using Modules.Records.Application.Locations.Commands.CreateLocation;
using Modules.Records.Application.Locations.Validators;

namespace Modules.Records.Application.Tests.Locations;

public sealed class CreateLocationCommandValidatorTests
{
    private readonly IValidator<CreateLocationCommand> _validator = new CreateLocationCommandValidator();

    #region Valid Commands

    [Fact]
    public void Validate_WithRequiredFieldsOnly_IsValid()
    {
        var command = new CreateLocationCommand("Main St", "Springfield");
        var result  = _validator.Validate(command);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithAllOptionalFields_IsValid()
    {
        var command = new CreateLocationCommand(
            StreetAddress:   "Oak Avenue",
            City:            "Shelbyville",
            StreetNumber:    "456",
            Zip:             "62701",
            AptSuite:        "Apt 3B",
            CommonPlaceName: "Town Hall",
            Comments:        "Corner of Oak and Elm");

        var result = _validator.Validate(command);
        Assert.True(result.IsValid);
    }

    #endregion

    #region StreetAddress Validation

    [Fact]
    public void Validate_EmptyStreetAddress_IsInvalid()
    {
        var command = new CreateLocationCommand("", "Springfield");
        var result  = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.StreetAddress));
    }

    [Fact]
    public void Validate_StreetAddressTooLong_IsInvalid()
    {
        var command = new CreateLocationCommand(new string('A', 201), "Springfield");
        var result  = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(command.StreetAddress) &&
            e.ErrorMessage.Contains("200"));
    }

    [Fact]
    public void Validate_StreetAddressExactlyMaxLength_IsValid()
    {
        var command = new CreateLocationCommand(new string('A', 200), "Springfield");
        var result  = _validator.Validate(command);
        Assert.True(result.IsValid);
    }

    #endregion

    #region City Validation

    [Fact]
    public void Validate_EmptyCity_IsInvalid()
    {
        var command = new CreateLocationCommand("Main St", "");
        var result  = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.City));
    }

    [Fact]
    public void Validate_CityTooLong_IsInvalid()
    {
        var command = new CreateLocationCommand("Main St", new string('C', 101));
        var result  = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(command.City) &&
            e.ErrorMessage.Contains("100"));
    }

    #endregion

    #region Optional Field Length Validation

    [Fact]
    public void Validate_StreetNumberTooLong_IsInvalid()
    {
        var command = new CreateLocationCommand("Main St", "Springfield",
            StreetNumber: new string('1', 21));
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.StreetNumber));
    }

    [Fact]
    public void Validate_ZipTooLong_IsInvalid()
    {
        var command = new CreateLocationCommand("Main St", "Springfield",
            Zip: new string('9', 11));
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Zip));
    }

    [Fact]
    public void Validate_AptSuiteTooLong_IsInvalid()
    {
        var command = new CreateLocationCommand("Main St", "Springfield",
            AptSuite: new string('A', 51));
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.AptSuite));
    }

    [Fact]
    public void Validate_CommonPlaceNameTooLong_IsInvalid()
    {
        var command = new CreateLocationCommand("Main St", "Springfield",
            CommonPlaceName: new string('P', 251));
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.CommonPlaceName));
    }

    [Fact]
    public void Validate_CommentsTooLong_IsInvalid()
    {
        var command = new CreateLocationCommand("Main St", "Springfield",
            Comments: new string('C', 501));
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Comments));
    }

    [Fact]
    public void Validate_NullOptionalFields_IsValid()
    {
        var command = new CreateLocationCommand("Main St", "Springfield",
            StreetNumber: null, Zip: null, AptSuite: null,
            CommonPlaceName: null, Comments: null);
        var result = _validator.Validate(command);
        Assert.True(result.IsValid);
    }

    #endregion
}
