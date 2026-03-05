using Modules.Records.Domain.DomainInvariants.IncidentClose;
using Modules.Records.Domain.Entities;
using Modules.Records.Domain.Factories;

namespace Modules.Records.Domain.Tests.DomainInvariants;

public sealed class CitationsMustBeIssuedInvariantTests
{
    private readonly CitationsMustBeIssuedInvariant _sut = new();

    private static Citation MakeCitation() =>
        new Citation(Guid.NewGuid(), Guid.NewGuid(), "Speeding", DateTime.UtcNow.AddDays(-1));

    private static IncidentCloseContext ContextWith(params Citation[] citations) =>
        new(new IncidentFactory().Create(Guid.NewGuid(), Guid.NewGuid(), "Test"), [], citations);

    [Fact]
    public void Check_NoCitations_ReturnsValid()
    {
        var result = _sut.Check(ContextWith());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Check_AllCitations_Issued_ReturnsValid()
    {
        var citation = MakeCitation();
        citation.Issue();

        var result = _sut.Check(ContextWith(citation));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Check_UnissuedCitation_ReturnsViolation()
    {
        var citation = MakeCitation(); // IsIssued = false by default

        var result = _sut.Check(ContextWith(citation));

        Assert.False(result.IsValid);
        Assert.Single(result.Violations);
        Assert.Equal(CitationsMustBeIssuedInvariant.Code, result.Violations[0].ErrorCode);
    }

    [Fact]
    public void Check_MixedIssuance_ReturnsViolation()
    {
        var issued = MakeCitation();
        issued.Issue();
        var unissued = MakeCitation();

        var result = _sut.Check(ContextWith(issued, unissued));

        Assert.False(result.IsValid);
        Assert.Contains("1 citation(s)", result.Violations[0].Reason);
    }
}

