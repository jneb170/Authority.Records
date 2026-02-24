using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Infrastructure.Outbox
{
    public sealed class OutboxCleanupOptions
    {
        public TimeSpan RetentionPeriod { get; init; } = TimeSpan.FromDays(7);

        public TimeSpan ExecutionInterval { get; init; } = TimeSpan.FromHours(6);
    }
}
