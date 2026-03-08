namespace Modules.Records.Application.Admin.Commands.RebuildReadModels;

public sealed record RebuildReadModelsResult(
    int      NamesRebuilt,
    int      ArrestsRebuilt,
    int      CitationsRebuilt,
    int      IncidentsRebuilt,
    int      ArrestLinksRebuilt,
    int      CitationLinksRebuilt,
    TimeSpan Elapsed);
