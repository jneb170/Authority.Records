using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Citations.Queries.GetCitationById;

public sealed class GetCitationByIdHandler : IRequestHandler<GetCitationByIdQuery, CitationDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCitationByIdHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CitationDto?> Handle(GetCitationByIdQuery request, CancellationToken cancellationToken)
    {
        var rm = await _dbContext.CitationReadModels
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CitationId, cancellationToken);

        if (rm is null)
            return null;

        return (await CitationDtoMapper.ToDtosAsync([rm], _dbContext, cancellationToken)).Single();
    }
}
