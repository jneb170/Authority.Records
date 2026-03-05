using MediatR;

namespace Modules.Records.Application.Citations.Commands.RestoreCitation;

public sealed record RestoreCitationCommand(Guid CitationId) : IRequest;
