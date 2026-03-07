using MediatR;

namespace Modules.Records.Application.Names.Commands.RestoreName;

public sealed record RestoreNameCommand(Guid NameId) : IRequest;
