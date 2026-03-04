using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Domain.Common.Implementations;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}