using Pochteca;
using Tests.Fakes;
using Xunit;

public class DedupeStoreTests
{
    [Fact]
    public async Task TryStamp_respects_ttl_and_tenant_and_endpoint_scope()
    {
        var start = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var clock = new FixedClock(start);
        var store = new InMemoryDedupeStore(clock);

        var tenantA = new TenantId(Guid.NewGuid());
        var tenantB = new TenantId(Guid.NewGuid());
        var endpoint = new EndpointKey("Loans.Amortize");
        var requestId = new RequestId("R1");
        var idempotencyKey = new IdempotencyKey("I1");
        var timeToLive = TimeSpan.FromMinutes(5);

        Assert.True(await store.TryStampAsync(tenantA, endpoint, requestId, idempotencyKey, timeToLive)); // first seen
        Assert.False(await store.TryStampAsync(tenantA, endpoint, requestId, idempotencyKey, timeToLive)); // duplicate within TTL
        Assert.True(await store.TryStampAsync(tenantB, endpoint, requestId, idempotencyKey, timeToLive)); // new tenant → new
        clock.Advance(timeToLive + TimeSpan.FromSeconds(1));
        Assert.True(await store.TryStampAsync(tenantA, endpoint, requestId, idempotencyKey, timeToLive)); // TTL expired → new again
    }
}
