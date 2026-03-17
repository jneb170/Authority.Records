using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Incidents.Queries.SearchIncidents;

/// <summary>
/// Full-text incident search. Matches against record number, incident number,
/// description, CFS number, and local number.
/// </summary>
public sealed record SearchIncidentsQuery(string? Term) : IRequest<IReadOnlyList<IncidentDto>>;
