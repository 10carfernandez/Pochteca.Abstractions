using Pochteca;

namespace Tests.Fakes;

internal sealed class FixedClock : IClock
{
    private DateTimeOffset _now;
    public FixedClock(DateTimeOffset start) => _now = start;
    public DateTimeOffset UtcNow => _now;
    public void Advance(TimeSpan by) => _now = _now + by;
}
