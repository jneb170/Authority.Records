using System.Runtime.CompilerServices;

// Allow Shared.Infrastructure to call internal audit-setter methods on AggregateRoot.
[assembly: InternalsVisibleTo("Shared.Infrastructure")]