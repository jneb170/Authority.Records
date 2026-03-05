using MediatR;

namespace Modules.Records.Application.Arrests.Commands.ArchiveArrest;

public sealed record ArchiveArrestCommand(Guid ArrestId) : IRequest;
