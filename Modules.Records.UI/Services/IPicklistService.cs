using Modules.Records.Application.DTOs;

namespace Modules.Records.UI.Services;

public interface IPicklistService
{
    /// <summary>Returns active picklist items for the given type (seeds defaults on first use).</summary>
    Task<IReadOnlyList<PicklistItemDto>> GetItemsAsync(string picklistType, bool activeOnly = true);

    /// <summary>Returns an Id → Label dictionary for a set of known IDs (for bulk list label resolution).</summary>
    Task<Dictionary<Guid, string>> GetItemsByIdsAsync(IReadOnlyList<Guid> ids);

    Task<PicklistSettingDto?> GetSettingAsync(string picklistType);
    Task SetSettingAsync(string picklistType, bool isRequired);

    Task<IReadOnlyList<string>> GetPicklistTypesAsync();

    Task<Guid> CreateItemAsync(string picklistType, string value, string label, int sortOrder);
    Task UpdateItemAsync(Guid itemId, string label, int sortOrder);
    Task DeactivateItemAsync(Guid itemId);
    Task ActivateItemAsync(Guid itemId);
}
