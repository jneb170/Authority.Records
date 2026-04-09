using MediatR;

namespace Modules.Records.Application.Charges.Queries.CountCharges;

public sealed record CountChargesQuery(bool IncludeInactive = false) : IRequest<int>;
