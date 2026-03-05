using MediatR;

namespace Modules.Records.Application.Arrests.Commands.FinalizeArrest;

public sealed record FinalizeArrestCommand(Guid ArrestId) : IRequest;
