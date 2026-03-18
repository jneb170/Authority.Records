using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Charges.Queries.GetChargesByCitation;

public sealed class GetChargesByCitationHandler : IRequestHandler<GetChargesByCitationQuery, IReadOnlyList<RecordChargeDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetChargesByCitationHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<RecordChargeDto>> Handle(GetChargesByCitationQuery request, CancellationToken cancellationToken)
    {
        return await (
            from link in _dbContext.CitationChargeLinks.AsNoTracking()
            join charge in _dbContext.Charges.AsNoTracking() on link.ChargeId equals charge.Id
            where link.CitationId == request.CitationId
            orderby charge.OffenseName, charge.UcrCode
            select new RecordChargeDto(
                charge.Id,
                charge.OffenseName,
                charge.UcrCode,
                charge.ChargeLevel,
                charge.StateClass,
                charge.IsCitationEligible,
                charge.IsActive,
                link.LinkedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
