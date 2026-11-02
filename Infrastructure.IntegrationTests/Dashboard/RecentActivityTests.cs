using Infrastructure.IntegrationTests.Projections;
using Modules.Records.Application.Arrests.Commands.CreateArrest;
using Modules.Records.Application.Common.Queries.GetRecentActivity;
using Modules.Records.Application.Names.Commands.CreateName;
using Modules.Records.Application.Names.Queries.GetNameByRecordNumber;

namespace Infrastructure.IntegrationTests.Dashboard;

/// <summary>
/// The dashboard feed is "your" records by default; IncludeAllUsers widens it to the
/// whole agency (used when a user, such as the shared demo account, has no activity yet).
/// Runs through the real MediatR pipeline so the read-model projections populate the feed.
/// </summary>
public sealed class RecentActivityTests : ProjectionHarness
{
    [Fact]
    public async Task RecordsByAnotherUser_AreHiddenByDefault_AndShownWithIncludeAllUsers()
    {
        var nameRecord = await Send(new CreateNameCommand(
            "Person", "Smith", "John", null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, false, null));
        var name = await Send(new GetNameByRecordNumberQuery(nameRecord));
        await Send(new CreateArrestCommand(name!.Id, DateTime.UtcNow.AddMinutes(-30), []));

        var someoneElse = Guid.NewGuid();

        var own = await Send(new GetRecentActivityQuery(someoneElse, JurisdictionId));
        Assert.Empty(own);

        var agency = await Send(new GetRecentActivityQuery(someoneElse, JurisdictionId, IncludeAllUsers: true));
        Assert.Contains(agency, a => a.RecordType == "Arrest");
        Assert.Contains(agency, a => a.RecordType == "Name");
    }
}
