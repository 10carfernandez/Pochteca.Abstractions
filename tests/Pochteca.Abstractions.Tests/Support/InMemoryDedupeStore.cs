using System.Collections.Concurrent;
using Pochteca;

namespace Tests.Fakes;

internal sealed class InMemoryDedupeStore : IUsageDedupeStore
{
    private readonly IClock _clock;
    private readonly ConcurrentDictionary<Key, DateTimeOffset> _expirations = new();

    public InMemoryDedupeStore(IClock clock) => _clock = clock;

    public Task<bool> TryStampAsync(
        TenantId tenant,
        EndpointKey endpoint,
        RequestId requestId,
        IdempotencyKey? idempotencyKey,
        TimeSpan timeToLive,
        CancellationToken cancellationToken = default)
    {
        var key = new Key(tenant, endpoint, requestId, idempotencyKey);
        var now = _clock.UtcNow;

        if (_expirations.TryGetValue(key, out var expiresAt) && expiresAt > now)
            return Task.FromResult(false);

        var newExpiry = now + timeToLive;
        _expirations.AddOrUpdate(key, newExpiry, (_, __) => newExpiry);

        var isNew = !(expiresAt > now);
        return Task.FromResult(isNew);
    }

    public Task<UsageEvent?> TryGetAsync(
        TenantId tenant,
        EndpointKey endpoint,
        RequestId requestId,
        IdempotencyKey? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        // This fake store does not persist events; return null.
        return Task.FromResult<UsageEvent?>(null);
    }

    private readonly record struct Key(TenantId Tenant, EndpointKey Endpoint, RequestId RequestId, IdempotencyKey? IdempotencyKey);
}
