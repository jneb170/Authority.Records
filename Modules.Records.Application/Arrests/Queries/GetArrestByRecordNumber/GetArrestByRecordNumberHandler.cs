using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Arrests.Queries.GetArrestByRecordNumber;

public sealed class GetArrestByRecordNumberHandler : IRequestHandler<GetArrestByRecordNumberQuery, ArrestDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetArrestByRecordNumberHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ArrestDto?> Handle(GetArrestByRecordNumberQuery request, CancellationToken cancellationToken)
    {
        var rm = await _dbContext.ArrestReadModels
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.RecordNumber == request.RecordNumber, cancellationToken);

        return rm?.ToDto();
    }
}
