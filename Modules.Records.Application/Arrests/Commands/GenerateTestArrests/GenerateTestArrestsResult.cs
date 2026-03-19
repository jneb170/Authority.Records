namespace Modules.Records.Application.Arrests.Commands.GenerateTestArrests;

public sealed record GenerateTestArrestsResult(
    int Created,
    int Failed,
    int NamesCreated,
    int NamesReusedFromExisting,
    int NamesReusedFromCurrentRun,
    int LocationsCreated,
    int LocationsReusedFromExisting,
    int LocationsReusedFromCurrentRun,
    IReadOnlyList<string> Errors);
