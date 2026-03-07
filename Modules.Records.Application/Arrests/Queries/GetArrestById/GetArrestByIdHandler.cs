using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Arrests.Queries.GetArrestById;

public sealed class GetArrestByIdHandler : IRequestHandler<GetArrestByIdQuery, ArrestDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetArrestByIdHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ArrestDto?> Handle(GetArrestByIdQuery request, CancellationToken cancellationToken)
    {
        var rm = await _dbContext.ArrestReadModels
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.ArrestId, cancellationToken);

        if (rm is null)
            return null;

        return rm.ToDto();
    }
}
