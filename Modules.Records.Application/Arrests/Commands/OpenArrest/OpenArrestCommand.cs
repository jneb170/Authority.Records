using MediatR;

namespace Modules.Records.Application.Arrests.Commands.OpenArrest;

public sealed record OpenArrestCommand(Guid ArrestId) : IRequest;
