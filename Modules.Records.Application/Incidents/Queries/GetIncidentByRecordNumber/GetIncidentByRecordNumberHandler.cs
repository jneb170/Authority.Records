using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Incidents.Queries.GetIncidentByRecordNumber;

public sealed class GetIncidentByRecordNumberHandler : IRequestHandler<GetIncidentByRecordNumberQuery, IncidentDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetIncidentByRecordNumberHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IncidentDto?> Handle(GetIncidentByRecordNumberQuery request, CancellationToken cancellationToken)
    {
        var rm = await _dbContext.IncidentReadModels
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.RecordNumber == request.RecordNumber, cancellationToken);

        return rm?.ToDto();
    }
}
