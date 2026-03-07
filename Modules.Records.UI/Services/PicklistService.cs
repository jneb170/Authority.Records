using MediatR;
using Modules.Records.Application.DTOs;
using Modules.Records.Application.Picklists.Commands.ActivatePicklistItem;
using Modules.Records.Application.Picklists.Commands.CreatePicklistItem;
using Modules.Records.Application.Picklists.Commands.DeactivatePicklistItem;
using Modules.Records.Application.Picklists.Commands.SetPicklistSetting;
using Modules.Records.Application.Picklists.Commands.UpdatePicklistItem;
using Modules.Records.Application.Picklists.Queries.GetPicklistItems;
using Modules.Records.Application.Picklists.Queries.GetPicklistItemsByIds;
using Modules.Records.Application.Picklists.Queries.GetPicklistSetting;
using Modules.Records.Application.Picklists.Queries.GetPicklistTypes;

namespace Modules.Records.UI.Services;

public sealed class PicklistService : IPicklistService
{
    private readonly ISender _sender;

    public PicklistService(ISender sender) => _sender = sender;

    public Task<IReadOnlyList<PicklistItemDto>> GetItemsAsync(string picklistType, bool activeOnly = true) =>
        _sender.Send(new GetPicklistItemsQuery(picklistType, activeOnly));

    public Task<Dictionary<Guid, string>> GetItemsByIdsAsync(IReadOnlyList<Guid> ids) =>
        _sender.Send(new GetPicklistItemsByIdsQuery(ids));

    public Task<PicklistSettingDto?> GetSettingAsync(string picklistType) =>
        _sender.Send(new GetPicklistSettingQuery(picklistType));

    public Task SetSettingAsync(string picklistType, bool isRequired) =>
        _sender.Send(new SetPicklistSettingCommand(picklistType, isRequired));

    public Task<IReadOnlyList<string>> GetPicklistTypesAsync() =>
        _sender.Send(new GetPicklistTypesQuery());

    public Task<Guid> CreateItemAsync(string picklistType, string value, string label, int sortOrder) =>
        _sender.Send(new CreatePicklistItemCommand(picklistType, value, label, sortOrder));

    public Task UpdateItemAsync(Guid itemId, string label, int sortOrder) =>
        _sender.Send(new UpdatePicklistItemCommand(itemId, label, sortOrder));

    public Task DeactivateItemAsync(Guid itemId) =>
        _sender.Send(new DeactivatePicklistItemCommand(itemId));

    public Task ActivateItemAsync(Guid itemId) =>
        _sender.Send(new ActivatePicklistItemCommand(itemId));
}
