using MediatR;

namespace Modules.Records.Application.Arrests.Commands.RestoreArrest;

public sealed record RestoreArrestCommand(Guid ArrestId) : IRequest;
