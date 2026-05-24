using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Common.Primitives;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Entities;
using Shared.Infrastructure.Audit;
using Shared.Infrastructure.Outbox;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;

namespace Shared.Infrastructure.Persistence;

public class AppDbContext : DbContext, IApplicationDbContext
{
    // RecordNumber values at or above this are treated as legacy random outliers
    // (from the original SQLite cutover's ABS(RANDOM()) default) rather than real
    // sequential numbers. Sequential numbers seed at 10000/20000 and increment by 1,
    // so realistic values stay far below this; ABS(RANDOM()) values are ~64-bit.
    // Used to keep outliers from poisoning the next sequential value, and by the
    // one-time repair that renumbers them. See SqliteRecordNumberRepair.
    internal const long RandomRecordNumberThreshold = 1_000_000_000L;

    protected readonly ITenantProvider _tenantProvider;
    protected readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly ILogger<AppDbContext> _logger;

    public Guid CurrentTenantId => _tenantProvider.GetJurisdictionId();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<DeadLetterMessage> DeadLetterMessages => Set<DeadLetterMessage>();
    public DbSet<AuditLogReadModel> AuditLogReadModels => Set<AuditLogReadModel>();

    // Read models
    public DbSet<IncidentReadModel> IncidentReadModels => Set<IncidentReadModel>();
    public DbSet<ArrestReadModel> ArrestReadModels => Set<ArrestReadModel>();
    public DbSet<CitationReadModel> CitationReadModels => Set<CitationReadModel>();
    public DbSet<NameReadModel> NameReadModels => Set<NameReadModel>();
    public DbSet<LocationReadModel> LocationReadModels => Set<LocationReadModel>();
    public DbSet<MugshotReadModel> MugshotReadModels => Set<MugshotReadModel>();
    public DbSet<MugshotLinkReadModel> MugshotLinkReadModels => Set<MugshotLinkReadModel>();
    public DbSet<NarrativeReadModel> NarrativeReadModels => Set<NarrativeReadModel>();
    public DbSet<NarrativeLinkReadModel> NarrativeLinkReadModels => Set<NarrativeLinkReadModel>();
    public DbSet<IncidentArrestLinkReadModel> IncidentArrestLinkReadModels => Set<IncidentArrestLinkReadModel>();
    public DbSet<IncidentCitationLinkReadModel> IncidentCitationLinkReadModels => Set<IncidentCitationLinkReadModel>();

    // Records module
    public DbSet<Incident> Incidents => Set<Incident>();
    public IQueryable<Incident> AllIncidentsWithDeleted => Set<Incident>().IgnoreQueryFilters();
    public DbSet<Arrest> Arrests => Set<Arrest>();
    public DbSet<ArrestNameSnapshot> ArrestNameSnapshots => Set<ArrestNameSnapshot>();
    public DbSet<Citation> Citations => Set<Citation>();
    public DbSet<CitationNameSnapshot> CitationNameSnapshots => Set<CitationNameSnapshot>();
    public DbSet<CitationOfficerProfile> CitationOfficerProfiles => Set<CitationOfficerProfile>();
    public DbSet<CitationTexasDetails> CitationTexasDetails => Set<CitationTexasDetails>();
    public DbSet<CitationVehicle> CitationVehicles => Set<CitationVehicle>();
    public DbSet<Charge> Charges => Set<Charge>();
    public DbSet<Name> Names => Set<Name>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Mugshot> Mugshots => Set<Mugshot>();
    public DbSet<MugshotLink> MugshotLinks => Set<MugshotLink>();
    public DbSet<Narrative> Narratives => Set<Narrative>();
    public DbSet<NarrativeLink> NarrativeLinks => Set<NarrativeLink>();
    public DbSet<IncidentArrestLink> IncidentArrestLinks => Set<IncidentArrestLink>();
    public DbSet<IncidentCitationLink> IncidentCitationLinks => Set<IncidentCitationLink>();
    public DbSet<ArrestChargeLink> ArrestChargeLinks => Set<ArrestChargeLink>();
    public DbSet<CitationChargeLink> CitationChargeLinks => Set<CitationChargeLink>();
    public DbSet<IncidentChargeLink> IncidentChargeLinks => Set<IncidentChargeLink>();

    public DbSet<JurisdictionConfiguration> JurisdictionConfigurations => Set<JurisdictionConfiguration>();
    public DbSet<AgencyConfiguration> AgencyConfigurations => Set<AgencyConfiguration>();
    public DbSet<AgencySequenceCounter> AgencySequenceCounters => Set<AgencySequenceCounter>();

    public DbSet<PicklistItem> PicklistItems => Set<PicklistItem>();
    public DbSet<PicklistSetting> PicklistSettings => Set<PicklistSetting>();

    public void Detach<TEntity>(TEntity entity) where TEntity : class
        => Entry(entity).State = EntityState.Detached;

    public AppDbContext(
        DbContextOptions<AppDbContext> options, 
        ITenantProvider tenantProvider, 
        IDomainEventDispatcher domainEventDispatcher,
        ILogger<AppDbContext>? logger = null)
        : base(options)
    {
        _tenantProvider = tenantProvider;
        _domainEventDispatcher = domainEventDispatcher;
        _logger = logger ?? NullLogger<AppDbContext>.Instance;
    }

    // Protected overload for derived test contexts (e.g. TestAppDbContext)
    protected AppDbContext(
        DbContextOptions options,
        ITenantProvider tenantProvider,
        IDomainEventDispatcher domainEventDispatcher,
        ILogger<AppDbContext>? logger = null)
        : base(options)
    {
        _tenantProvider = tenantProvider;
        _domainEventDispatcher = domainEventDispatcher;
        _logger = logger ?? NullLogger<AppDbContext>.Instance;
    }

    public override int SaveChanges()
    {
        return SaveChangesAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Asynchronously saves all changes made in this context to the underlying database, while collecting
    /// domain events from aggregate roots and storing them in the outbox for reliable processing.
    /// </summary>
    /// <remarks>
    /// This method implements the Outbox pattern for handling domain events:
    /// <list type="number">
    /// <item>Updates row versions for optimistic concurrency control.</item>
    /// <item>Collects domain events from tracked aggregate roots that have been added, modified, or deleted.</item>
    /// <item>Persists domain events to the OutboxMessages table for reliable, transactional event processing.</item>
    /// <item>Saves all changes to the database in a single transaction.</item>
    /// <item>Clears domain events from aggregate roots after successful persistence.</item>
    /// </list>
    /// This ensures that domain events are durably stored alongside the entity changes, preventing event loss
    /// and enabling reliable eventual consistency across the system.
    /// </remarks>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous save operation.</param>
    /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries
    /// written to the database.</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateRowVersions();
        AssignSqliteRecordNumbers();
        Guid? tenantId = null;

        var domainEntities = ChangeTracker.Entries<AggregateRoot>()
            .Where(e =>
                e.State == EntityState.Added ||
                e.State == EntityState.Modified ||
                e.State == EntityState.Deleted)
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(e => e.Entity.DomainEvents)
            .OfType<IDomainEvent>()
            .ToList();


        //TODO: Find a better way to differentiate between Non-Tenant and Tenant services
        // Only call CurrentTenantId when there are domain events to write.
        // Background services (e.g. LockCleanupService) may call SaveChangesAsync
        // without an HTTP context — those saves carry no domain events and must not
        // attempt to resolve the tenant from the request pipeline.
        if (domainEvents.Count > 0)
        {
            tenantId = CurrentTenantId;

            var aggregatesById = domainEntities
                .Select(x => x.Entity)
                .GroupBy(x => x.Id)
                .ToDictionary(x => x.Key, x => x.Last());

            foreach (var domainEvent in domainEvents)
            {
                OutboxMessages.Add(new OutboxMessage(domainEvent, tenantId.Value));

                aggregatesById.TryGetValue(domainEvent.AggregateId, out var aggregate);
                AuditLogReadModels.Add(AuditLogEntryFactory.CreateFromDomainEvent(
                    domainEvent,
                    aggregate,
                    tenantId.Value));
            }
        }

        // EF Core handles the transaction automatically for SaveChangesAsync
        var result = await base.SaveChangesAsync(cancellationToken);

        // Clear events only after successful save
        foreach (var entity in domainEntities)
        {
            entity.Entity.ClearDomainEvents();
        }

        // Dispatch synchronously for immediate in-process consistency.
        // Projection handlers are idempotent, so the outbox processor's later
        // dispatch is a safe no-op.
        if (domainEvents.Count > 0)
        {
            try
            {
                await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
            }
            catch (Exception ex)
            {
                var aggregateIds = domainEvents
                    .Select(domainEvent => domainEvent.AggregateId)
                    .Distinct()
                    .ToArray();
                var eventTypes = domainEvents
                    .Select(domainEvent => domainEvent.GetType().Name)
                    .Distinct()
                    .ToArray();

                _logger.LogError(
                    ex,
                    "Immediate domain event dispatch failed after commit. Write model changes succeeded and the outbox will retry asynchronously. TenantId: {TenantId}; AggregateIds: {AggregateIds}; EventTypes: {EventTypes}",
                    tenantId,
                    aggregateIds,
                    eventTypes);
            }
        }

        return result;
    }

    /// <summary>
    /// SQLite has no IDENTITY columns, so RecordNumber (a SqlServer identity column)
    /// is assigned here for newly added aggregates. Mirrors the SqlServer seeds
    /// (10000 for most aggregates, 20000 for Location) and increments by one, keeping
    /// the short, sequential, URL-friendly numbers users expect. No-op on SqlServer.
    /// </summary>
    private void AssignSqliteRecordNumbers()
    {
        if (!Database.IsSqlite())
            return;

        AssignSqliteRecordNumbers<Incident>(seed: 10000);
        AssignSqliteRecordNumbers<Arrest>(seed: 10000);
        AssignSqliteRecordNumbers<Citation>(seed: 10000);
        AssignSqliteRecordNumbers<Name>(seed: 10000);
        AssignSqliteRecordNumbers<Location>(seed: 20000);
        AssignSqliteRecordNumbers<Narrative>(seed: 30000);
    }

    private void AssignSqliteRecordNumbers<T>(long seed) where T : class
    {
        var added = ChangeTracker.Entries<T>()
            .Where(e => e.State == EntityState.Added
                        && (long)e.Property("RecordNumber").CurrentValue! <= 0)
            .ToList();

        if (added.Count == 0)
            return;

        // Highest existing sequential number for this entity, ignoring soft-delete /
        // tenant filters (RecordNumber is unique table-wide) and ignoring any legacy
        // random outliers so they don't inflate the next value.
        var max = Set<T>()
            .IgnoreQueryFilters()
            .Where(x => EF.Property<long>(x, "RecordNumber") < RandomRecordNumberThreshold)
            .Select(x => (long?)EF.Property<long>(x, "RecordNumber"))
            .Max() ?? (seed - 1);

        foreach (var entry in added)
            entry.Property("RecordNumber").CurrentValue = ++max;
    }

    private void UpdateRowVersions()
    {
        //var entries = ChangeTracker.Entries<AggregateRoot>()
        //    .Where(e => e.State == EntityState.Modified);
        //changed to below. When AggregateRoot type is specified,
        //  then Entites like OutboxMessage don't populate RowVersion
        // Include Added so SQLite (which lacks server-generated rowversion) gets a seed value.
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added);

        foreach (var entry in entries)
        {
            if (entry.Metadata.FindProperty("RowVersion") is null)
                continue;

            var prop = entry.Property("RowVersion");
            prop.CurrentValue = Guid.NewGuid().ToByteArray();
        }
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ApplyGlobalFilters(modelBuilder);

        ApplyJurisdictionConfiguration(modelBuilder);

        // Automatically apply all IEntityTypeConfiguration
        // These should be defined in the same assembly as AppDbContext and
        //  implement IEntityTypeConfiguration<T> for each entity type.
        // Store these configurations in Persistence/Configurations folder for better organization.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        if (Database.IsSqlite())
        {
            ApplySqliteOverrides(modelBuilder);
        }
    }

    private static void ApplySqliteOverrides(ModelBuilder modelBuilder)
    {
        // SQLite has no IDENTITY columns on non-PK columns. SqlServer uses
        // UseIdentityColumn(seed, increment) to give RecordNumber short, sequential
        // values (e.g. 10000, 10001, ...) that are surfaced directly in URLs.
        // We reproduce that under SQLite by assigning RecordNumber sequentially in
        // AssignSqliteRecordNumbers() during SaveChanges, so the column is
        // application-generated (ValueGeneratedNever) rather than DB-defaulted.
        modelBuilder.Entity<Incident>().Property(x => x.RecordNumber).ValueGeneratedNever();
        modelBuilder.Entity<Arrest>().Property(x => x.RecordNumber).ValueGeneratedNever();
        modelBuilder.Entity<Citation>().Property(x => x.RecordNumber).ValueGeneratedNever();
        modelBuilder.Entity<Name>().Property(x => x.RecordNumber).ValueGeneratedNever();
        modelBuilder.Entity<Location>().Property(x => x.RecordNumber).ValueGeneratedNever();
        modelBuilder.Entity<Narrative>().Property(x => x.RecordNumber).ValueGeneratedNever();

        // datetime2 is SQL Server-specific; SQLite stores DateTime as TEXT.
        modelBuilder.Entity<Incident>().Property(x => x.OccurredOn).HasColumnType("TEXT");
        modelBuilder.Entity<IncidentReadModel>().Property(x => x.OccurredOn).HasColumnType("TEXT");

        // SQLite does not support filtered indexes. Drop the HasFilter on this unique index.
        // Caveat: at most one Incident per (Jurisdiction, Agency) may have a blank IncidentNum.
        // Demo-volume usage is well within tolerance; the sequence counter assigns real numbers quickly.
        modelBuilder.Entity<Incident>()
            .HasIndex(x => new { x.JurisdictionId, x.AgencyId, x.IncidentNum })
            .IsUnique()
            .HasFilter(null);

        // IsRowVersion() emits a SQL Server 'rowversion' column. SQLite has no equivalent.
        // Demote to plain concurrency-token BLOB; UpdateRowVersions() seeds the value
        // with Guid.NewGuid().ToByteArray() on Added/Modified.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var rowVersionProp = entityType.FindProperty("RowVersion");
            if (rowVersionProp is null)
                continue;

            var columnType = rowVersionProp.GetColumnType();
            var isSqlServerRowVersion =
                rowVersionProp.ValueGenerated == ValueGenerated.OnAddOrUpdate
                && (string.Equals(columnType, "rowversion", System.StringComparison.OrdinalIgnoreCase)
                    || string.Equals(columnType, "timestamp", System.StringComparison.OrdinalIgnoreCase));

            if (isSqlServerRowVersion || rowVersionProp.ClrType == typeof(byte[]))
            {
                rowVersionProp.ValueGenerated = ValueGenerated.Never;
                rowVersionProp.IsConcurrencyToken = true;
                rowVersionProp.SetColumnType("BLOB");
            }
        }
    }


    private void ApplyGlobalFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            var parameter = Expression.Parameter(clrType, "e");
            Expression? filterExpression = null;

            //Soft Delete Filter (IsDeleted): AggregateRoot
            if (typeof(AggregateRoot).IsAssignableFrom(clrType))
            {
                var isDeletedProperty = Expression.Property(parameter, nameof(AggregateRoot.IsDeleted));
                var notDeleted = Expression.Equal(isDeletedProperty, Expression.Constant(false));

                filterExpression = AppendFilterExpression(filterExpression, notDeleted);
            }

            //Multi-Tenant Filter (JurisdictionId): IMultiTenant
            if (typeof(AggregateRoot).IsAssignableFrom(clrType))
            {
                var jurisdictionProperty = Expression.Property(parameter, nameof(IMultiTenant.JurisdictionId));

                var currentTenantProperty = Expression.Property(
                    Expression.Constant(this),
                    nameof(CurrentTenantId));

                var tenantMatch = Expression.Equal(jurisdictionProperty, currentTenantProperty);

                filterExpression = AppendFilterExpression(filterExpression, tenantMatch);
            }

            // Apply combined query filters
            if (filterExpression != null)
            {
                var lamda = Expression.Lambda(filterExpression, parameter);
                modelBuilder.Entity(clrType).HasQueryFilter(lamda);
            }
        }
    }

    private void ApplyJurisdictionConfiguration(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JurisdictionConfiguration>(builder =>
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.JurisdictionId)
                   .IsUnique();

            builder.Property(x => x.MustCloseAllArrests)
                   .IsRequired();

            builder.Property(x => x.MustCloseAllCitations)
                   .IsRequired();
        });

        // AgencySequenceCounter is not an AggregateRoot so the global filter doesn't apply; add tenant filter manually
        modelBuilder.Entity<AgencySequenceCounter>()
            .HasQueryFilter(c => c.JurisdictionId == CurrentTenantId);
    }


    /// <summary>
    /// Combines the specified filter expression with an existing expression using a logical AND operation.
    /// </summary>
    /// <remarks>This method is useful for building composite filter expressions dynamically. The resulting
    /// expression can be used in LINQ queries or expression trees to represent combined filtering logic.</remarks>
    /// <param name="currentExpression">The existing filter expression to which the new expression will be appended. If null, the method returns the
    /// appended expression as the initial filter.</param>
    /// <param name="expressionToAppend">The filter expression to append to the current expression using a logical AND.</param>
    /// <returns>An expression representing the logical AND of the current expression and the appended filter expression. If the
    /// current expression is null, returns the appended expression.</returns>
    private static Expression AppendFilterExpression(Expression? currentExpression, BinaryExpression expressionToAppend)
    {
        currentExpression = currentExpression == null
                            ? expressionToAppend
                            : Expression.AndAlso(currentExpression, expressionToAppend);
        return currentExpression;
    }
}


