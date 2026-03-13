using MediatR;

namespace Modules.Records.Application.Locations.Commands.SoftDeleteLocation;

public sealed record SoftDeleteLocationCommand(Guid LocationId) : IRequest;
