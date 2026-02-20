using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Entities;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Shared.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext, IApplicationDbContext
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    // Records module
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<Arrest> Arrests => Set<Arrest>();
    public DbSet<Citation> Citations => Set<Citation>();

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider, IDomainEventDispatcher domainEventDispatcher)
        : base(options)
    {
        _tenantProvider = tenantProvider;
        _domainEventDispatcher = domainEventDispatcher;
    }

    public override int SaveChanges()
    {
        UpdateRowVersions();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateRowVersions();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateRowVersions()
    {
        var entries = ChangeTracker.Entries<Incident>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            entry.Property(x => x.RowVersion).CurrentValue =
                Guid.NewGuid().ToByteArray();
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
            if (typeof(IMultiTenant).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AppDbContext)
                    .GetMethod(nameof(SetTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .MakeGenericMethod(entityType.ClrType);

                method.Invoke(this, new object[] { modelBuilder });
            }

            // Soft delete filter for entities that have IsDeleted
            if (entityType.ClrType.GetProperty("IsDeleted") != null)
            {
                var method = typeof(AppDbContext)
                    .GetMethod(nameof(SetSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Instance)!
                    .MakeGenericMethod(entityType.ClrType);

                method.Invoke(this, new object[] { modelBuilder });
            }
        }
    }

    private void SetTenantFilter<TEntity>(ModelBuilder modelBuilder)
    where TEntity : class, IMultiTenant
    {
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(e => e.JurisdictionId == _tenantProvider.GetJurisdictionId());
    }

    private void SetSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
    where TEntity : AggregateRoot
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }

}
