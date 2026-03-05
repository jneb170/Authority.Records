using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.Entities;
using System.Collections.Generic;

namespace Modules.Records.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<Incident> Incidents { get; }
    DbSet<Arrest> Arrests { get; }
    DbSet<Citation> Citations { get; }

    DbSet<IncidentReadModel> IncidentReadModels { get; }
    DbSet<ArrestReadModel> ArrestReadModels { get; }
    DbSet<CitationReadModel> CitationReadModels { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
