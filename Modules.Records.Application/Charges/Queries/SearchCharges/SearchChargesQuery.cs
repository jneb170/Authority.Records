using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Charges.Queries.SearchCharges;

public sealed record SearchChargesQuery(
    string? Term = null,
    bool IncludeInactive = false,
    bool CitationEligibleOnly = false) : IRequest<IReadOnlyList<ChargeDto>>;
