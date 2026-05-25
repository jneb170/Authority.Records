using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Common.Violations;
using Modules.Records.UI.Services;

namespace Modules.Records.UI.Tests.Printing;

/// <summary>Minimal hand-built stubs — the builder only calls one method on each service. Everything
/// else throws so an accidental new dependency surfaces loudly in tests.</summary>
internal sealed class FakeCitationService : ICitationService
{
    private readonly CitationDto? _citation;
    public FakeCitationService(CitationDto? citation) => _citation = citation;

    public Task<CitationDto?> GetByRecordNumberAsync(long recordNumber) => Task.FromResult(_citation);

    public Task<CitationDto?> GetByIdAsync(Guid id) => throw new NotImplementedException();
    public Task<IReadOnlyList<CitationDto>> GetByJurisdictionAsync() => throw new NotImplementedException();
    public Task<IReadOnlyList<CitationDto>> GetByIncidentAsync(Guid incidentId) => throw new NotImplementedException();
    public Task<IReadOnlyList<IncidentCitationLinkDto>> GetLinkedIncidentsAsync(Guid citationId) => throw new NotImplementedException();
    public Task<long> CreateAsync(string description, DateTime issueDate, IReadOnlyList<long> incidentRecordNumbers, string citationNum = "") => throw new NotImplementedException();
    public Task<long> CreateAsync(Guid incidentId, string description, DateTime issueDate) => throw new NotImplementedException();
    public Task LinkToIncidentAsync(Guid citationId, Guid incidentId) => throw new NotImplementedException();
    public Task UnlinkFromIncidentAsync(Guid citationId, Guid incidentId) => throw new NotImplementedException();
    public Task IssueAsync(Guid id) => throw new NotImplementedException();
    public Task UpdateDetailsAsync(Guid id, Guid? defendantNameId, string description, DateTime issueDate, Guid? courtId = null, string citationNum = "", Guid? locationId = null, NameSnapshotInput? atTimeOfName = null, CitationOfficerProfileInput? officerProfile = null, CitationTexasDetailsInput? texasDetails = null, CitationVehicleInput? vehicle = null) => throw new NotImplementedException();
    public Task SavePageAsync(Guid id, Guid? defendantNameId, string description, DateTime issueDate, Guid? courtId = null, string citationNum = "", Guid? locationId = null, NameSnapshotInput? atTimeOfName = null, CitationOfficerProfileInput? officerProfile = null, CitationTexasDetailsInput? texasDetails = null, CitationVehicleInput? vehicle = null, CitationOffenseDetailsInput? offenseDetails = null, IReadOnlyCollection<ViolationFlagKey>? violationFlags = null, IReadOnlyCollection<Guid>? incidentIdsToAdd = null, IReadOnlyCollection<Guid>? incidentIdsToRemove = null, IReadOnlyCollection<Guid>? chargeIdsToAdd = null, IReadOnlyCollection<Guid>? chargeIdsToRemove = null) => throw new NotImplementedException();
    public Task AcquireLockAsync(Guid id) => throw new NotImplementedException();
    public Task RenewLockAsync(Guid id) => throw new NotImplementedException();
    public Task ReleaseLockAsync(Guid id) => throw new NotImplementedException();
    public Task SoftDeleteAsync(Guid id) => throw new NotImplementedException();
    public Task RestoreAsync(Guid id) => throw new NotImplementedException();
}

internal sealed class FakePicklistService : IPicklistService
{
    private readonly Dictionary<Guid, string> _labels;
    public FakePicklistService(Dictionary<Guid, string> labels) => _labels = labels;

    public Task<Dictionary<Guid, string>> GetItemsByIdsAsync(IReadOnlyList<Guid> ids)
        => Task.FromResult(ids.Where(_labels.ContainsKey).ToDictionary(id => id, id => _labels[id]));

    public Task<IReadOnlyList<PicklistItemDto>> GetItemsAsync(string picklistType, bool activeOnly = true) => throw new NotImplementedException();
    public Task<PicklistSettingDto?> GetSettingAsync(string picklistType) => throw new NotImplementedException();
    public Task SetSettingAsync(string picklistType, bool isRequired) => throw new NotImplementedException();
    public Task<IReadOnlyList<string>> GetPicklistTypesAsync() => throw new NotImplementedException();
    public Task<Guid> CreateItemAsync(string picklistType, string value, string label, int sortOrder) => throw new NotImplementedException();
    public Task UpdateItemAsync(Guid itemId, string label, int sortOrder) => throw new NotImplementedException();
    public Task DeactivateItemAsync(Guid itemId) => throw new NotImplementedException();
    public Task ActivateItemAsync(Guid itemId) => throw new NotImplementedException();
}

internal sealed class FakeLocationService : ILocationService
{
    private readonly Dictionary<Guid, LocationDto> _locations;
    public FakeLocationService(Dictionary<Guid, LocationDto>? locations = null) => _locations = locations ?? new();

    public Task<LocationDto?> GetByIdAsync(Guid id) => Task.FromResult(_locations.GetValueOrDefault(id));

    public Task<LocationDto?> GetByRecordNumberAsync(long recordNumber) => throw new NotImplementedException();
    public Task<IReadOnlyList<LocationDto>> GetByJurisdictionAsync() => throw new NotImplementedException();
    public Task<IReadOnlyList<LocationDto>> SearchAsync(string? addressContains = null, string? city = null, Guid? stateId = null, string? zip = null, string? commonPlaceName = null) => throw new NotImplementedException();
    public Task<long> CreateAsync(string streetAddress, string city, string? streetNumber = null, Guid? preDirectionId = null, Guid? streetTypeId = null, Guid? postDirectionId = null, Guid? stateId = null, Guid? countryId = null, string? zip = null, string? aptSuite = null, string? coordinates = null, string? commonPlaceName = null, string? comments = null, string? address = null) => throw new NotImplementedException();
    public Task UpdateDetailsAsync(Guid locationId, string streetAddress, string city, string? streetNumber = null, Guid? preDirectionId = null, Guid? streetTypeId = null, Guid? postDirectionId = null, Guid? stateId = null, Guid? countryId = null, string? zip = null, string? aptSuite = null, string? coordinates = null, string? commonPlaceName = null, string? comments = null, string? address = null) => throw new NotImplementedException();
    public Task AcquireLockAsync(Guid id) => throw new NotImplementedException();
    public Task RenewLockAsync(Guid id) => throw new NotImplementedException();
    public Task ReleaseLockAsync(Guid id) => throw new NotImplementedException();
    public Task SoftDeleteAsync(Guid id) => throw new NotImplementedException();
    public Task RestoreAsync(Guid id) => throw new NotImplementedException();
}
