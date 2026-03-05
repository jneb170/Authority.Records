using MediatR;

namespace Modules.Records.Application.Citations.Commands.SoftDeleteCitation;

public sealed record SoftDeleteCitationCommand(Guid CitationId) : IRequest;
