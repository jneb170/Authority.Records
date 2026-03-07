using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Citations.Queries.GetCitationByRecordNumber;

public sealed class GetCitationByRecordNumberHandler : IRequestHandler<GetCitationByRecordNumberQuery, CitationDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCitationByRecordNumberHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CitationDto?> Handle(GetCitationByRecordNumberQuery request, CancellationToken cancellationToken)
    {
        var rm = await _dbContext.CitationReadModels
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.RecordNumber == request.RecordNumber, cancellationToken);

        return rm?.ToDto();
    }
}
