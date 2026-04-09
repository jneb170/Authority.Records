namespace Shared.Infrastructure.Maintenance;

public sealed class ApplicationActivityTracker : IApplicationActivityTracker
{
    private readonly object _signalGate = new();
    private TaskCompletionSource<DateTime> _nextActivitySignal = CreateSignal();
    private long _lastActivityTicks = DateTime.UtcNow.Ticks;

    public DateTime LastActivityUtc => new(Interlocked.Read(ref _lastActivityTicks), DateTimeKind.Utc);

    public bool HasRecentActivity(TimeSpan threshold, DateTime nowUtc)
        => nowUtc - LastActivityUtc < threshold;

    public void RecordActivity(DateTime? activityUtc = null)
    {
        var utcTimestamp = activityUtc?.ToUniversalTime() ?? DateTime.UtcNow;
        var currentTicks = Interlocked.Read(ref _lastActivityTicks);
        if (utcTimestamp.Ticks <= currentTicks)
            return;

        Interlocked.Exchange(ref _lastActivityTicks, utcTimestamp.Ticks);

        lock (_signalGate)
        {
            _nextActivitySignal.TrySetResult(utcTimestamp);
            _nextActivitySignal = CreateSignal();
        }
    }

    public Task WaitForActivityAsync(DateTime observedLastActivityUtc, CancellationToken cancellationToken)
    {
        if (LastActivityUtc > observedLastActivityUtc)
            return Task.CompletedTask;

        Task<DateTime> waitTask;

        lock (_signalGate)
        {
            if (LastActivityUtc > observedLastActivityUtc)
                return Task.CompletedTask;

            waitTask = _nextActivitySignal.Task;
        }

        return waitTask.WaitAsync(cancellationToken);
    }

    private static TaskCompletionSource<DateTime> CreateSignal()
        => new(TaskCreationOptions.RunContinuationsAsynchronously);
}
