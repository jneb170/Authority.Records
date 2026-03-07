using MediatR;

namespace Modules.Records.Application.Names.Commands.SoftDeleteName;

public sealed record SoftDeleteNameCommand(Guid NameId) : IRequest;
