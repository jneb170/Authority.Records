using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<Incident> Incidents { get; }
    DbSet<Arrest> Arrests { get; }
    DbSet<Citation> Citations { get; }
    DbSet<IncidentArrestLink> IncidentArrestLinks { get; }
    DbSet<IncidentCitationLink> IncidentCitationLinks { get; }

    DbSet<IncidentReadModel> IncidentReadModels { get; }
    DbSet<ArrestReadModel> ArrestReadModels { get; }
    DbSet<CitationReadModel> CitationReadModels { get; }
    DbSet<IncidentArrestLinkReadModel> IncidentArrestLinkReadModels { get; }
    DbSet<IncidentCitationLinkReadModel> IncidentCitationLinkReadModels { get; }

    DbSet<AgencyConfiguration> AgencyConfigurations { get; }
    DbSet<AgencySequenceCounter> AgencySequenceCounters { get; }

    DbSet<PicklistItem> PicklistItems { get; }
    DbSet<PicklistSetting> PicklistSettings { get; }

    /// <summary>Detaches an entity from the change tracker (used for concurrency retry scenarios).</summary>
    void Detach<TEntity>(TEntity entity) where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
