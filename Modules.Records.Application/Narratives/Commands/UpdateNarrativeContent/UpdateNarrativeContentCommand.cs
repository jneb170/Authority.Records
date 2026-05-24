using MediatR;

namespace Modules.Records.Application.Narratives.Commands.UpdateNarrativeContent;

public sealed record UpdateNarrativeContentCommand(
    Guid NarrativeId,
    string Title,
    string Content) : IRequest;
