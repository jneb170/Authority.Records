using Modules.Records.Application.DTOs;

namespace Modules.Records.UI.Services;

public interface IAgencyConfigurationService
{
    Task<IReadOnlyList<AgencyConfigurationDto>> GetAllAsync();
    Task<AgencyConfigurationDto?> GetAsync(string key);
    Task SetAsync(string key, string value);
    Task DeleteAsync(string key);

    /// <summary>
    /// Atomically reserves and returns the next formatted IncidentNum for the current agency.
    /// </summary>
    Task<string> GenerateIncidentNumAsync();

    /// <summary>
    /// Atomically reserves and returns the next formatted ArrestNum for the current agency.
    /// </summary>
    Task<string> GenerateArrestNumAsync();

    /// <summary>
    /// Atomically reserves and returns the next formatted CitationNum for the current agency.
    /// </summary>
    Task<string> GenerateCitationNumAsync();
}

