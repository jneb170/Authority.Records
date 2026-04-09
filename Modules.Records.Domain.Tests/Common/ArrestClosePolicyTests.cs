using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Exceptions;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Common.Policies;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Tests.Common;

public sealed class ArrestClosePolicyTests
{
    [Fact]
    public void ValidateCanClose_WithoutLinkedName_ThrowsAggregateDomainException()
    {
        var arrest = new Arrest(Guid.NewGuid(), Guid.NewGuid(), null, DateTime.UtcNow.AddDays(-1), string.Empty, null);
        var sut = new ArrestClosePolicy(new StubJurisdictionRules());

        var ex = Assert.Throws<AggregateDomainException>(() => sut.ValidateCanClose(arrest, isForced: false));

        Assert.Contains(ex.Errors, e => e.Message.Contains("Linked name required"));
    }

    [Fact]
    public void ValidateCanClose_WithLinkedName_DoesNotThrow()
    {
        var arrest = new Arrest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(-1), string.Empty, null);
        var sut = new ArrestClosePolicy(new StubJurisdictionRules());

        var ex = Record.Exception(() => sut.ValidateCanClose(arrest, isForced: false));

        Assert.Null(ex);
    }

    private sealed class StubJurisdictionRules : IJurisdictionRulesService
    {
        public bool MustCloseAllArrests(Guid jurisdictionId) => false;
        public bool MustCloseAllCitations(Guid jurisdictionId) => false;
    }
}
