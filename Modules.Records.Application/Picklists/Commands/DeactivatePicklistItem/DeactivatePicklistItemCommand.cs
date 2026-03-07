using MediatR;

namespace Modules.Records.Application.Picklists.Commands.DeactivatePicklistItem;

public sealed record DeactivatePicklistItemCommand(Guid ItemId) : IRequest;
