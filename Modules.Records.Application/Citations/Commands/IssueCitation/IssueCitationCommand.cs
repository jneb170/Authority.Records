using MediatR;

namespace Modules.Records.Application.Citations.Commands.IssueCitation;

public sealed record IssueCitationCommand(Guid CitationId) : IRequest;
