using MediatR;

namespace Modules.Records.Application.Narratives.Commands.SoftDeleteNarrative;

public sealed record SoftDeleteNarrativeCommand(Guid NarrativeId) : IRequest;
