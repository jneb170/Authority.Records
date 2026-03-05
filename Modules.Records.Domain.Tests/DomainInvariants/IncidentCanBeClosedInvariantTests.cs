using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Common.Policies;
using Modules.Records.Domain.DomainInvariants.IncidentClose;
using Modules.Records.Domain.Entities;
using Modules.Records.Domain.Factories;

namespace Modules.Records.Domain.Tests.DomainInvariants;

public sealed class IncidentCanBeClosedInvariantTests
{
    private static IncidentCanBeClosedInvariant MakeSut(bool mustCloseArrests) =>
        new(new StubJurisdictionRules(mustCloseArrests));

    private static Arrest MakeArrest() =>
        new ArrestFactory().Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Test Subject", DateTime.UtcNow.AddDays(-1));

    private static Arrest MakeFinalizedClosedArrest()
    {
        var arrest = MakeArrest();
        var ctx = new UserModificationContext(Guid.NewGuid());
        var policy = new DefaultLifecyclePolicy<Arrest>(new DefaultClosePolicy<Arrest>());
        arrest.Open(ctx, policy);
        arrest.Close(ctx, policy);
        arrest.Finalize();
        return arrest;
    }

    private static Citation MakeIssuedCitation()
    {
        var c = new Citation(Guid.NewGuid(), Guid.NewGuid(), "Test");
        c.Issue();
        return c;
    }

    private static IncidentCloseContext EmptyContext() =>
        new(new IncidentFactory().Create(Guid.NewGuid(), Guid.NewGuid(), "Test"), [], []);

    [Fact]
    public void Check_NoChildRecords_ReturnsValid()
    {
        var result = MakeSut(mustCloseArrests: false).Check(EmptyContext());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Check_AllSatisfied_ReturnsValid()
    {
        var arrest = MakeFinalizedClosedArrest();
        var citation = MakeIssuedCitation();
        var context = new IncidentCloseContext(
            new IncidentFactory().Create(Guid.NewGuid(), Guid.NewGuid(), "Test"),
            [arrest], [citation]);

        var result = MakeSut(mustCloseArrests: true).Check(context);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Check_UnfinalizedArrest_ReturnsViolation()
    {
        var arrest = MakeArrest(); // not finalized
        var context = new IncidentCloseContext(
            new IncidentFactory().Create(Guid.NewGuid(), Guid.NewGuid(), "Test"),
            [arrest], []);

        var result = MakeSut(mustCloseArrests: false).Check(context);

        Assert.False(result.IsValid);
        Assert.Contains(result.Violations, v => v.ErrorCode == ArrestsMustBeFinalizedInvariant.Code);
    }

    [Fact]
    public void Check_UnissuedCitation_ReturnsViolation()
    {
        var citation = new Citation(Guid.NewGuid(), Guid.NewGuid(), "Test"); // not issued
        var context = new IncidentCloseContext(
            new IncidentFactory().Create(Guid.NewGuid(), Guid.NewGuid(), "Test"),
            [], [citation]);

        var result = MakeSut(mustCloseArrests: false).Check(context);

        Assert.False(result.IsValid);
        Assert.Contains(result.Violations, v => v.ErrorCode == CitationsMustBeIssuedInvariant.Code);
    }

    [Fact]
    public void Check_MustCloseArrests_OpenArrest_ReturnsViolationIncludingClosed()
    {
        var arrest = MakeArrest(); // Draft — not closed and not finalized
        var context = new IncidentCloseContext(
            new IncidentFactory().Create(Guid.NewGuid(), Guid.NewGuid(), "Test"),
            [arrest], []);

        var result = MakeSut(mustCloseArrests: true).Check(context);

        Assert.False(result.IsValid);
        Assert.Equal(2, result.Violations.Count); // finalized + closed
        Assert.Contains(result.Violations, v => v.ErrorCode == ArrestsMustBeFinalizedInvariant.Code);
        Assert.Contains(result.Violations, v => v.ErrorCode == ArrestsMustBeClosedInvariant.Code);
    }

    [Fact]
    public void Check_MustCloseArrests_False_DoesNotCheckClosed()
    {
        // Arrest is finalized but not closed — should be valid when mustClose=false
        var arrest = MakeArrest();
        arrest.Finalize();
        var context = new IncidentCloseContext(
            new IncidentFactory().Create(Guid.NewGuid(), Guid.NewGuid(), "Test"),
            [arrest], []);

        var result = MakeSut(mustCloseArrests: false).Check(context);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Check_MultipleViolations_AllCollected()
    {
        var arrest = MakeArrest();       // not finalized
        var citation = new Citation(Guid.NewGuid(), Guid.NewGuid(), "Speeding"); // not issued
        var context = new IncidentCloseContext(
            new IncidentFactory().Create(Guid.NewGuid(), Guid.NewGuid(), "Test"),
            [arrest], [citation]);

        var result = MakeSut(mustCloseArrests: false).Check(context);

        Assert.Equal(2, result.Violations.Count);
    }

    private sealed class StubJurisdictionRules : IJurisdictionRulesService
    {
        private readonly bool _mustCloseArrests;

        public StubJurisdictionRules(bool mustCloseArrests)
        {
            _mustCloseArrests = mustCloseArrests;
        }

        public bool MustCloseAllArrests(Guid jurisdictionId) => _mustCloseArrests;
        public bool MustCloseAllCitations(Guid jurisdictionId) => false;
    }
}
