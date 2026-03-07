using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Incidents.Queries.GetIncidentById;

public sealed class GetIncidentByIdHandler : IRequestHandler<GetIncidentByIdQuery, IncidentDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetIncidentByIdHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IncidentDto?> Handle(GetIncidentByIdQuery request, CancellationToken cancellationToken)
    {
        var rm = await _dbContext.IncidentReadModels
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == request.IncidentId, cancellationToken);

        return rm?.ToDto();
    }
}
