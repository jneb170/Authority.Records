using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<Incident> Incidents { get; }
    /// <summary>Queryable that bypasses the global IsDeleted filter — use for rebuild/admin operations only.</summary>
    IQueryable<Incident> AllIncidentsWithDeleted { get; }
    DbSet<Arrest> Arrests { get; }
    DbSet<ArrestNameSnapshot> ArrestNameSnapshots { get; }
    DbSet<Citation> Citations { get; }
    DbSet<CitationNameSnapshot> CitationNameSnapshots { get; }
    DbSet<CitationOfficerProfile> CitationOfficerProfiles { get; }
    DbSet<CitationTexasDetails> CitationTexasDetails { get; }
    DbSet<CitationOffenseDetails> CitationOffenseDetails { get; }
    DbSet<CitationViolationFlag> CitationViolationFlags { get; }
    DbSet<CitationVehicle> CitationVehicles { get; }
    DbSet<Charge> Charges { get; }
    DbSet<Name> Names { get; }
    DbSet<Location> Locations { get; }
    DbSet<Mugshot> Mugshots { get; }
    DbSet<MugshotLink> MugshotLinks { get; }
    DbSet<Narrative> Narratives { get; }
    DbSet<NarrativeLink> NarrativeLinks { get; }
    DbSet<IncidentArrestLink> IncidentArrestLinks { get; }
    DbSet<IncidentCitationLink> IncidentCitationLinks { get; }
    DbSet<ArrestChargeLink> ArrestChargeLinks { get; }
    DbSet<CitationChargeLink> CitationChargeLinks { get; }
    DbSet<IncidentChargeLink> IncidentChargeLinks { get; }

    DbSet<IncidentReadModel> IncidentReadModels { get; }
    DbSet<ArrestReadModel> ArrestReadModels { get; }
    DbSet<CitationReadModel> CitationReadModels { get; }
    DbSet<NameReadModel> NameReadModels { get; }
    DbSet<LocationReadModel> LocationReadModels { get; }
    DbSet<MugshotReadModel> MugshotReadModels { get; }
    DbSet<MugshotLinkReadModel> MugshotLinkReadModels { get; }
    DbSet<NarrativeReadModel> NarrativeReadModels { get; }
    DbSet<NarrativeLinkReadModel> NarrativeLinkReadModels { get; }
    DbSet<IncidentArrestLinkReadModel> IncidentArrestLinkReadModels { get; }
    DbSet<IncidentCitationLinkReadModel> IncidentCitationLinkReadModels { get; }
    DbSet<AuditLogReadModel> AuditLogReadModels { get; }

    DbSet<AgencyConfiguration> AgencyConfigurations { get; }
    DbSet<AgencySequenceCounter> AgencySequenceCounters { get; }

    DbSet<PicklistItem> PicklistItems { get; }
    DbSet<PicklistSetting> PicklistSettings { get; }

    /// <summary>Detaches an entity from the change tracker (used for concurrency retry scenarios).</summary>
    void Detach<TEntity>(TEntity entity) where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
