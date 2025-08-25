using Pochteca;

namespace Tests.Fakes;

internal sealed class CollectingUsageSink : IUsageSink
{
    private readonly List<UsageEvent> _events = new();
    public IReadOnlyList<UsageEvent> Events => _events;

    public Task WriteAsync(IReadOnlyList<UsageEvent> batch, CancellationToken ct = default)
    {
        _events.AddRange(batch);
        return Task.CompletedTask;
    }
}
