using Modules.Records.Domain.ValueObjects;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Policies;
using Modules.Records.Domain.DomainInvariants.IncidentClose;
using Modules.Records.Domain.Entities;
using Modules.Records.Domain.Factories;

namespace Modules.Records.Domain.Tests.DomainInvariants;

public sealed class ArrestsMustBeFinalizedInvariantTests
{
    private readonly ArrestsMustBeFinalizedInvariant _sut = new();

    private static Arrest MakeArrest() =>
        new ArrestFactory().Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(-1), string.Empty);

    private static IncidentCloseContext ContextWith(params Arrest[] arrests) =>
        new(new IncidentFactory().Create(new CreateIncidentRequest { JurisdictionId = Guid.NewGuid(), AgencyId = Guid.NewGuid(), Details = new IncidentDetails { IncidentNum = "INC-001", Description = "Test", LocalNum = "" } }), arrests, []);

    [Fact]
    public void Check_NoArrests_ReturnsValid()
    {
        var result = _sut.Check(ContextWith());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Check_AllArrests_Finalized_ReturnsValid()
    {
        var arrest = MakeArrest();
        arrest.Finalize();

        var result = _sut.Check(ContextWith(arrest));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Check_UnfinalizedArrest_ReturnsViolation()
    {
        var arrest = MakeArrest(); // IsFinalized = false by default

        var result = _sut.Check(ContextWith(arrest));

        Assert.False(result.IsValid);
        Assert.Single(result.Violations);
        Assert.Equal(ArrestsMustBeFinalizedInvariant.Code, result.Violations[0].ErrorCode);
    }

    [Fact]
    public void Check_MixedFinalization_ReturnsViolation()
    {
        var finalized = MakeArrest();
        finalized.Finalize();
        var unfinalized = MakeArrest();

        var result = _sut.Check(ContextWith(finalized, unfinalized));

        Assert.False(result.IsValid);
        Assert.Contains("1 arrest(s)", result.Violations[0].Reason);
    }
}



