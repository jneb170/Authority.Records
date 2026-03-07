using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Primitives;

namespace Shared.Infrastructure.Audit;

public sealed class AuditInterceptor : SaveChangesInterceptor
{
    private readonly IModificationContext _modificationContext;

    public AuditInterceptor(IModificationContext modificationContext)
    {
        _modificationContext = modificationContext;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        var now    = DateTime.UtcNow;
        var userId = _modificationContext.UserId;

        foreach (var entry in context.ChangeTracker.Entries<AggregateRoot>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.SetCreatedAudit(userId, now);
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.SetModifiedAudit(userId, now);
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
