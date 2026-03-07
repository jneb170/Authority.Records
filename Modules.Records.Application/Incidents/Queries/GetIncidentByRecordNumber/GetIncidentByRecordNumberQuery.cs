using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Incidents.Queries.GetIncidentByRecordNumber;

public sealed record GetIncidentByRecordNumberQuery(long RecordNumber) : IRequest<IncidentDto?>;
