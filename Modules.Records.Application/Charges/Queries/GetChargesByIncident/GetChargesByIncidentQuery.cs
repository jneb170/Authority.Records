using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Charges.Queries.GetChargesByIncident;

public sealed record GetChargesByIncidentQuery(Guid IncidentId) : IRequest<IReadOnlyList<RecordChargeDto>>;
