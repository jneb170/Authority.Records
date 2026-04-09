using Modules.Records.Domain.ValueObjects;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Common.Policies;
using Modules.Records.Domain.DomainInvariants.IncidentClose;
using Modules.Records.Domain.Entities;
using Modules.Records.Domain.Factories;

namespace Modules.Records.Domain.Tests.DomainInvariants;

public sealed class ArrestsMustBeClosedInvariantTests
{
    private readonly ArrestsMustBeClosedInvariant _sut = new();

    private static Arrest MakeArrest() =>
        new ArrestFactory().Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(-2), string.Empty);

    private static Arrest MakeClosedArrest()
    {
        var arrest = MakeArrest();
        var ctx = new UserModificationContext(Guid.NewGuid());
        var policy = new DefaultLifecyclePolicy<Arrest>(new DefaultClosePolicy<Arrest>());
        arrest.Open(ctx, policy);
        arrest.Close(ctx, policy);
        return arrest;
    }

    private static IncidentCloseContext ContextWith(params Arrest[] arrests) =>
        new(new IncidentFactory().Create(new CreateIncidentRequest { JurisdictionId = Guid.NewGuid(), AgencyId = Guid.NewGuid(), Details = new IncidentDetails { IncidentNum = "INC-001", Description = "Test", LocalNum = "" } }), arrests, []);

    [Fact]
    public void Check_NoArrests_ReturnsValid()
    {
        var result = _sut.Check(ContextWith());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Check_AllArrests_Closed_ReturnsValid()
    {
        var arrest = MakeClosedArrest();

        var result = _sut.Check(ContextWith(arrest));

        Assert.True(result.IsValid);
        Assert.Equal(RecordStatus.Closed, arrest.Status);
    }

    [Fact]
    public void Check_DraftArrest_ReturnsViolation()
    {
        var arrest = MakeArrest(); // Status = Draft

        var result = _sut.Check(ContextWith(arrest));

        Assert.False(result.IsValid);
        Assert.Single(result.Violations);
        Assert.Equal(ArrestsMustBeClosedInvariant.Code, result.Violations[0].ErrorCode);
    }

    [Fact]
    public void Check_OpenArrest_ReturnsViolation()
    {
        var arrest = MakeArrest();
        var ctx = new UserModificationContext(Guid.NewGuid());
        var policy = new DefaultLifecyclePolicy<Arrest>(new DefaultClosePolicy<Arrest>());
        arrest.Open(ctx, policy);

        var result = _sut.Check(ContextWith(arrest));

        Assert.False(result.IsValid);
        Assert.Contains("1 arrest(s)", result.Violations[0].Reason);
    }
}



