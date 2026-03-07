using MediatR;

namespace Modules.Records.Application.Picklists.Commands.UpdatePicklistItem;

public sealed record UpdatePicklistItemCommand(
    Guid   ItemId,
    string Label,
    int    SortOrder) : IRequest;
