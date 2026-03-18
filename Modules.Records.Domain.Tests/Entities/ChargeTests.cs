using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Tests.Entities;

public sealed class ChargeTests
{
    private static readonly Guid TestJurisdictionId = Guid.NewGuid();
    private static readonly Guid TestAgencyId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithRequiredFields_SetsProperties()
    {
        var charge = CreateCharge();

        Assert.Equal(TestJurisdictionId, charge.JurisdictionId);
        Assert.Equal(TestAgencyId, charge.AgencyId);
        Assert.Equal("Assault Causing Bodily Injury", charge.OffenseName);
        Assert.Equal("13A", charge.UcrCode);
        Assert.Equal("Felony", charge.ChargeLevel);
        Assert.True(charge.IsActive);
        Assert.False(charge.IsDeleted);
    }

    [Fact]
    public void Constructor_RaisesCreatedDomainEvent()
    {
        var charge = CreateCharge();

        var evt = Assert.Single(charge.DomainEvents.OfType<ChargeCreatedDomainEvent>());
        Assert.Equal(charge.Id, evt.ChargeId);
        Assert.Equal("Assault Causing Bodily Injury", evt.OffenseName);
        Assert.Equal("13A", evt.UcrCode);
    }

    [Fact]
    public void Update_ChangesFields_AndRaisesUpdatedEvent()
    {
        var charge = CreateCharge();
        charge.ClearDomainEvents();

        charge.Update(
            "Public Intoxication",
            "Part II",
            "Group B",
            "Society",
            "90E",
            "Misdemeanor",
            "Class C",
            true);

        Assert.Equal("Public Intoxication", charge.OffenseName);
        Assert.Equal("90E", charge.UcrCode);
        Assert.Equal("Class C", charge.StateClass);
        Assert.True(charge.IsCitationEligible);

        var evt = Assert.Single(charge.DomainEvents.OfType<ChargeUpdatedDomainEvent>());
        Assert.Equal(charge.Id, evt.ChargeId);
        Assert.Equal("Public Intoxication", evt.OffenseName);
    }

    [Fact]
    public void Deactivate_ThenActivate_TogglesActiveFlag_AndRaisesEvents()
    {
        var charge = CreateCharge();
        charge.ClearDomainEvents();

        charge.Deactivate();

        Assert.False(charge.IsActive);
        Assert.Single(charge.DomainEvents.OfType<ChargeDeactivatedDomainEvent>());

        charge.ClearDomainEvents();
        charge.Activate();

        Assert.True(charge.IsActive);
        Assert.Single(charge.DomainEvents.OfType<ChargeActivatedDomainEvent>());
    }

    private static Charge CreateCharge() =>
        new(
            TestJurisdictionId,
            TestAgencyId,
            "Assault Causing Bodily Injury",
            "Part I",
            "Group A",
            "Person",
            "13A",
            "Felony",
            null,
            false);
}
