using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;

namespace Modules.Records.Application.Mugshots.Commands.SetPrimaryMugshot;

public sealed class SetPrimaryMugshotHandler : IRequestHandler<SetPrimaryMugshotCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public SetPrimaryMugshotHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(SetPrimaryMugshotCommand request, CancellationToken cancellationToken)
    {
        var links = await _dbContext.MugshotLinks
            .Where(l => l.OwnerType == request.OwnerType && l.OwnerId == request.OwnerId)
            .ToListAsync(cancellationToken);

        var target = links.FirstOrDefault(l => l.MugshotId == request.MugshotId)
            ?? throw new InvalidOperationException("Mugshot link not found.");

        foreach (var link in links)
        {
            link.SetPrimary(link.Id == target.Id);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
