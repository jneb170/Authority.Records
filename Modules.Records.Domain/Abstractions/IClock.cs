namespace Modules.Records.Domain.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
}
