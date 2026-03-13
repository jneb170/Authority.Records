using MediatR;

namespace Modules.Records.Application.Locations.Commands.RestoreLocation;

public sealed record RestoreLocationCommand(Guid LocationId) : IRequest;
