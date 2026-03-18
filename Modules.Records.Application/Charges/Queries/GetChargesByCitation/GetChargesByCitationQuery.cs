using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Charges.Queries.GetChargesByCitation;

public sealed record GetChargesByCitationQuery(Guid CitationId) : IRequest<IReadOnlyList<RecordChargeDto>>;
