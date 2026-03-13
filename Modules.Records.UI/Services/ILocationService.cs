using Modules.Records.Application.DTOs;

namespace Modules.Records.UI.Services;

public interface ILocationService
{
    Task<LocationDto?> GetByIdAsync(Guid id);
    Task<LocationDto?> GetByRecordNumberAsync(long recordNumber);
    Task<IReadOnlyList<LocationDto>> GetByJurisdictionAsync();
    Task<IReadOnlyList<LocationDto>> SearchAsync(
        string? addressContains = null,
        string? city            = null,
        Guid?   stateId         = null,
        string? zip             = null,
        string? commonPlaceName = null);
    Task<long> CreateAsync(
        string  streetAddress,
        string  city,
        string? streetNumber    = null,
        Guid?   preDirectionId  = null,
        Guid?   streetTypeId    = null,
        Guid?   postDirectionId = null,
        Guid?   stateId         = null,
        Guid?   countryId       = null,
        string? zip             = null,
        string? aptSuite        = null,
        string? coordinates     = null,
        string? commonPlaceName = null,
        string? comments        = null);
    Task UpdateDetailsAsync(
        Guid    locationId,
        string  streetAddress,
        string  city,
        string? streetNumber    = null,
        Guid?   preDirectionId  = null,
        Guid?   streetTypeId    = null,
        Guid?   postDirectionId = null,
        Guid?   stateId         = null,
        Guid?   countryId       = null,
        string? zip             = null,
        string? aptSuite        = null,
        string? coordinates     = null,
        string? commonPlaceName = null,
        string? comments        = null);
    Task AcquireLockAsync(Guid id);
    Task ReleaseLockAsync(Guid id);
    Task SoftDeleteAsync(Guid id);
    Task RestoreAsync(Guid id);
}
