namespace Modules.Records.Application.Abstractions;

public interface IGoogleMapsPlacesClient
{
    Task<IReadOnlyList<GooglePlaceResult>> SearchAsync(
        string            keyword,
        int               maxResults,
        string            apiKey,
        CancellationToken cancellationToken = default);
}
