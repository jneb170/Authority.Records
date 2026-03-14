namespace Modules.Records.UI.Services;

public interface IGoogleMapsConfigService
{
    /// <summary>Returns the Google Maps API key for the current jurisdiction, or null if not configured.</summary>
    Task<string?> GetApiKeyAsync();

    /// <summary>Clears the cached API key so the next call re-reads from the database.</summary>
    void InvalidateCache();
}
