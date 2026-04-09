using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Charges.Queries.GetChargesByArrest;

public sealed record GetChargesByArrestQuery(Guid ArrestId) : IRequest<IReadOnlyList<RecordChargeDto>>;
