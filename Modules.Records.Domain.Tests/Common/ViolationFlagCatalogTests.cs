using Modules.Records.Domain.Common.Violations;

namespace Modules.Records.Domain.Tests.Common;

public sealed class ViolationFlagCatalogTests
{
    [Fact]
    public void Catalog_HasOneDefinition_PerEnumValue()
    {
        var enumValues = Enum.GetValues<ViolationFlagKey>();

        Assert.Equal(enumValues.Length, ViolationFlagCatalog.Definitions.Count);
        Assert.Equal(
            enumValues.OrderBy(k => k).ToArray(),
            ViolationFlagCatalog.Definitions.Select(d => d.Key).OrderBy(k => k).ToArray());
    }

    [Fact]
    public void Catalog_HasNoDuplicateKeys()
    {
        var keys = ViolationFlagCatalog.Definitions.Select(d => d.Key).ToList();

        Assert.Equal(keys.Count, keys.Distinct().Count());
    }

    [Fact]
    public void EveryDefinition_HasANonEmptyLabel()
    {
        Assert.All(ViolationFlagCatalog.Definitions, d => Assert.False(string.IsNullOrWhiteSpace(d.Label)));
    }

    [Theory]
    [InlineData(ViolationFlagKey.NoSignal, "No Signal", ViolationFlagSection.Offense)]
    [InlineData(ViolationFlagKey.Ice, "Ice", ViolationFlagSection.Contributor)]
    [InlineData(ViolationFlagKey.DodgeDriver, "Driver", ViolationFlagSection.Dodge)]
    [InlineData(ViolationFlagKey.HitFixedObject, "Hit fixed object", ViolationFlagSection.Collision)]
    public void Label_And_Section_MatchCatalog(ViolationFlagKey key, string expectedLabel, ViolationFlagSection expectedSection)
    {
        Assert.Equal(expectedLabel, ViolationFlagCatalog.Label(key));
        Assert.Equal(expectedSection, ViolationFlagCatalog.Section(key));
    }

    [Fact]
    public void ForSection_ReturnsOnlyThatSection_InDeclarationOrder()
    {
        var offense = ViolationFlagCatalog.ForSection(ViolationFlagSection.Offense);

        Assert.NotEmpty(offense);
        Assert.All(offense, d => Assert.Equal(ViolationFlagSection.Offense, d.Section));
        Assert.Equal(ViolationFlagKey.UnreasonableForConditions, offense[0].Key);
    }

    [Fact]
    public void EverySection_HasAtLeastOneFlag()
    {
        foreach (var section in Enum.GetValues<ViolationFlagSection>())
            Assert.NotEmpty(ViolationFlagCatalog.ForSection(section));
    }
}
