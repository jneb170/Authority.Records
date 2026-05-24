using MediatR;
using Modules.Records.Application.Common;

namespace Modules.Records.Application.Narratives.Commands.CreateNarrative;

/// <summary>Creates a narrative document and links it to an owner (Incident/Arrest/Citation).</summary>
public sealed record CreateNarrativeCommand(
    string OwnerType,
    Guid OwnerId,
    string Title,
    string Content) : IRequest<long>, IRateLimitedCommand;
