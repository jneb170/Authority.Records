using MediatR;

namespace Modules.Records.Application.Picklists.Commands.ActivatePicklistItem;

public sealed record ActivatePicklistItemCommand(Guid ItemId) : IRequest;
