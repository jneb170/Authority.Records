namespace Modules.Records.Application.Arrests.Commands.GenerateTestArrests;

public enum TestDataRecordLinkStrategy
{
    Existing = 0,
    CreateNew = 1,
    RecentlyCreatedOrCreateNew = 2
}
