using Modules.Records.Application.Names.Validators;

namespace Modules.Records.Application.Tests.Names;

public sealed class SocialSecurityNumberRulesTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("212-55-1234")]
    [InlineData("212551234")]
    [InlineData("899-99-9999")]
    public void Accepts_blank_and_structurally_valid_numbers(string? ssn) =>
        Assert.True(SocialSecurityNumberRules.IsValid(ssn));

    [Theory]
    [InlineData("078-05-1120")]   // Woolworth wallet-insert number
    [InlineData("219-09-9999")]   // Social Security Board pamphlet number
    [InlineData("987-65-4321")]   // advertising range (also area 9xx)
    [InlineData("000-12-3456")]   // area 000
    [InlineData("666-12-3456")]   // area 666
    [InlineData("900-12-3456")]   // area 900-999
    [InlineData("123-00-4567")]   // group 00
    [InlineData("123-45-0000")]   // serial 0000
    [InlineData("12-345-6789")]   // wrong shape
    [InlineData("123-45-678")]
    [InlineData("abc-de-fghi")]
    public void Rejects_invalid_numbers(string ssn) =>
        Assert.False(SocialSecurityNumberRules.IsValid(ssn));
}
