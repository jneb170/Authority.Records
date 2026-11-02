using Modules.Records.Domain.Common.Implementations;

namespace Modules.Records.Domain.Tests.Implementations;

public class AgencySequenceCounterTests
{
    private static AgencySequenceCounter NewCounter() => new(Guid.NewGuid(), Guid.NewGuid(), "IncidentFormat", 2026);

    [Fact]
    public void EnsureAtLeast_MovesTheCounterPastExistingNumbers()
    {
        var counter = NewCounter();

        Assert.True(counter.EnsureAtLeast(4595));
        Assert.Equal(4596, counter.Increment());
    }

    [Fact]
    public void EnsureAtLeast_NeverMovesTheCounterBack()
    {
        var counter = NewCounter();
        counter.EnsureAtLeast(10);

        Assert.False(counter.EnsureAtLeast(3));
        Assert.Equal(11, counter.Increment());
    }
}
