using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Shared.Infrastructure.Audit;

public sealed class AuditInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;

        if (context is null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                // Example audit logic
                // entry.Property("CreatedOn").CurrentValue = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                // entry.Property("ModifiedOn").CurrentValue = DateTime.UtcNow;
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
