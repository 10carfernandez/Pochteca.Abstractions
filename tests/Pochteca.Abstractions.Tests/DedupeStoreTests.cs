using Pochteca;
using Tests.Fakes;
using Xunit;

public class DedupeStoreTests
{
    [Fact]
    public async Task TryStamp_respects_ttl_and_tenant_scope()
    {
        var start = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var clock = new FixedClock(start);
        var store = new InMemoryDedupeStore(clock);

        var tenantA = new TenantId(Guid.NewGuid());
        var tenantB = new TenantId(Guid.NewGuid());
        var rid = new RequestId("R1");
        var idem = new IdempotencyKey("I1");
        var ttl = TimeSpan.FromMinutes(5);

        Assert.True(await store.TryStampAsync(tenantA, rid, idem, ttl)); // first seen
        Assert.False(await store.TryStampAsync(tenantA, rid, idem, ttl)); // duplicate within TTL
        Assert.True(await store.TryStampAsync(tenantB, rid, idem, ttl)); // new tenant → new
        clock.Advance(ttl + TimeSpan.FromSeconds(1));
        Assert.True(await store.TryStampAsync(tenantA, rid, idem, ttl)); // TTL expired → new again
    }
}
