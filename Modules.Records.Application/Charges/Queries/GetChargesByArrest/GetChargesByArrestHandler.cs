using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Charges.Queries.GetChargesByArrest;

public sealed class GetChargesByArrestHandler : IRequestHandler<GetChargesByArrestQuery, IReadOnlyList<RecordChargeDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetChargesByArrestHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<RecordChargeDto>> Handle(GetChargesByArrestQuery request, CancellationToken cancellationToken)
    {
        return await (
            from link in _dbContext.ArrestChargeLinks.AsNoTracking()
            join charge in _dbContext.Charges.AsNoTracking() on link.ChargeId equals charge.Id
            where link.ArrestId == request.ArrestId
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
