using System.Collections.Concurrent;
using Pochteca;

namespace Tests.Fakes;

internal sealed class InMemoryDedupeStore : IUsageDedupeStore
{
    private readonly IClock _clock;
    private readonly ConcurrentDictionary<Key, DateTimeOffset> _expirations = new();

    public InMemoryDedupeStore(IClock clock) => _clock = clock;

    public Task<bool> TryStampAsync(TenantId tenant, RequestId requestId, IdempotencyKey? idempotencyKey, TimeSpan ttl, CancellationToken ct = default)
    {
        var key = new Key(tenant, requestId, idempotencyKey);
        var now = _clock.UtcNow;

        // Fast path: check current value
        if (_expirations.TryGetValue(key, out var expiresAt) && expiresAt > now)
            return Task.FromResult(false);

        var newExpiry = now + ttl;

        // Atomically set/update
		_expirations.AddOrUpdate(key, newExpiry, (_, __) => newExpiry);

        // Re-check: if previously un-stamped or expired, this is considered new
        var isNew = !(expiresAt > now);
        return Task.FromResult(isNew);
    }

    private readonly record struct Key(TenantId Tenant, RequestId RequestId, IdempotencyKey? IdempotencyKey);
}
