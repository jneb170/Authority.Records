using MediatR;

namespace Modules.Records.Application.Picklists.Commands.CreatePicklistItem;

public sealed record CreatePicklistItemCommand(
    string PicklistType,
    string Value,
    string Label,
    int    SortOrder) : IRequest<Guid>;
