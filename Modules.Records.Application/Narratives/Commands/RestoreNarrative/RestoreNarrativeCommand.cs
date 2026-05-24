using MediatR;

namespace Modules.Records.Application.Narratives.Commands.RestoreNarrative;

public sealed record RestoreNarrativeCommand(Guid NarrativeId) : IRequest;
