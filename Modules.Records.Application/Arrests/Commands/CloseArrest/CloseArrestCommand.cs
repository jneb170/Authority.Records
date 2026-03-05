using MediatR;

namespace Modules.Records.Application.Arrests.Commands.CloseArrest;

public sealed record CloseArrestCommand(Guid ArrestId, bool Force = false) : IRequest;
