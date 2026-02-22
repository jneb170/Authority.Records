using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Entities;
using Shared.Infrastructure.Outbox;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Shared.Infrastructure.Persistence;

public class AppDbContext : DbContext, IApplicationDbContext
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public Guid CurrentTenantId => _tenantProvider.GetJurisdictionId();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    // Records module
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<Arrest> Arrests => Set<Arrest>();
    public DbSet<Citation> Citations => Set<Citation>();

    public AppDbContext(
        DbContextOptions options, 
        ITenantProvider tenantProvider, 
        IDomainEventDispatcher domainEventDispatcher)
        : base(options)
    {
        _tenantProvider = tenantProvider;
        _domainEventDispatcher = domainEventDispatcher;
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

        var tenantId = CurrentTenantId; //DbContext property bound to ITenantProvider

        foreach (var domainEvent in domainEvents)
        {
            OutboxMessages.Add(new OutboxMessage(domainEvent, tenantId));
        }

        // EF Core handles the transaction automatically for SaveChangesAsync
        var result = await base.SaveChangesAsync(cancellationToken);

        // Clear events only after successful save
        foreach (var entity in domainEntities)
        {
            entity.Entity.ClearDomainEvents();
        }

        return result;
    }

    private void UpdateRowVersions()
    {
        var entries = ChangeTracker.Entries<AggregateRoot>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var prop = entry.Property("RowVersion");
            if (prop != null)
                prop.CurrentValue = Guid.NewGuid().ToByteArray();
        }
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ApplyGlobalFilters(modelBuilder);

        // Automatically apply all IEntityTypeConfiguration
        // These should be defined in the same assembly as AppDbContext and
        //  implement IEntityTypeConfiguration<T> for each entity type.
        // Store these configurations in Persistence/Configurations folder for better organization.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

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
