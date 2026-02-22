using MediatR;

namespace Infrastructure.IntegrationTests.Outbox.RetryBehavior;

internal sealed class AlwaysFailingHandler
    : INotificationHandler<FailingDomainEvent>
{
    public static int ExecutionCount { get; private set; }

    public static void Reset() => ExecutionCount = 0;

    public Task Handle(FailingDomainEvent notification, CancellationToken cancellationToken)
    {
        ExecutionCount++;
        throw new InvalidOperationException("Simulated failure.");
    }
}
