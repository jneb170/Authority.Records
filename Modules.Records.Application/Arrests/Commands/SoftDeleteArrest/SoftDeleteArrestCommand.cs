using MediatR;

namespace Modules.Records.Application.Arrests.Commands.SoftDeleteArrest;

public sealed record SoftDeleteArrestCommand(Guid ArrestId) : IRequest;
