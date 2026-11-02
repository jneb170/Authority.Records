using Modules.Records.Application.Arrests.Commands.CreateArrest;
using Modules.Records.Application.Common.Queries.GetDashboardCounts;
using Modules.Records.Application.Names.Commands.CreateName;
using Modules.Records.Application.Names.Queries.GetNameByRecordNumber;
using Infrastructure.IntegrationTests.Projections;

namespace Infrastructure.IntegrationTests.Dashboard;

/// <summary>
/// Issue #100: the dashboard tiles counted rows in the 20-item activity feed. They now come from real counts.
/// </summary>
public sealed class DashboardCountsTests : ProjectionHarness
{
    [Fact]
    public async Task Counts_RecordsCreatedSinceTheCutoff_NotCappedByTheActivityFeed()
    {
        var nameRecord = await Send(new CreateNameCommand(
            "Person", "Smith", "John", null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, false, null));
        var name = await Send(new GetNameByRecordNumberQuery(nameRecord));
        for (var i = 0; i < 22; i++)
            await Send(new CreateArrestCommand(name!.Id, DateTime.UtcNow.AddMinutes(-30 - i), []));

        var counts = await Send(new GetDashboardCountsQuery(DateTime.UtcNow.AddDays(-7)));

        Assert.Equal(22, counts.NewArrests); // more than the feed's 20 rows
        Assert.Equal(0, counts.NewIncidents);
        Assert.Equal(0, counts.NewCitations);
        Assert.Equal(0, counts.OpenIncidents);
    }

    [Fact]
    public async Task Counts_ExcludeRecordsCreatedBeforeTheCutoff()
    {
        var nameRecord = await Send(new CreateNameCommand(
            "Person", "Doe", "Jane", null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, false, null));
        var name = await Send(new GetNameByRecordNumberQuery(nameRecord));
        await Send(new CreateArrestCommand(name!.Id, DateTime.UtcNow.AddMinutes(-30), []));

        var counts = await Send(new GetDashboardCountsQuery(DateTime.UtcNow.AddMinutes(5)));

        Assert.Equal(0, counts.NewArrests);
    }
}
