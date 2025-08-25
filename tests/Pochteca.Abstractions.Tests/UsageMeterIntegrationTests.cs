using Pochteca;
using Tests.Fakes;
using Xunit;

public class UsageMeterIntegrationTests
{
    [Fact]
    public async Task TryRecord_calculates_dedupes_and_writes_once()
    {
        var rules = new[] { new UnitRule("/v1/loans", 1.00m, 0.02m, "periods") };
        var calc = new RuleBasedUnitCalculator(new SimplePrefixUnitRuleProvider(rules));

        var clock = new FixedClock(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero));
        var store = new InMemoryDedupeStore(clock);
        var sink = new CollectingUsageSink();
        var meter = new UsageMeter(calc, store, sink, clock, ttl: TimeSpan.FromHours(24));

        var tenant = new TenantId(Guid.NewGuid());
        var rid = new RequestId("req-123");
        var idem = new IdempotencyKey("idem-abc");
        var req = new RequestInfo(
            "POST",
            "/v1/loans/amortize",
            clock.UtcNow,
            new EndpointKey("Loans.Amortize"),
            200,
            new Dictionary<string, object?> { ["periods"] = 36 });

        var first = await meter.TryRecordAsync(tenant, req, idem, rid);
        var second = await meter.TryRecordAsync(tenant, req, idem, rid);

        Assert.NotNull(first);
        Assert.Null(second); // deduped
        Assert.Single(sink.Events);
        Assert.Equal(1.72m, sink.Events[0].Units); // 1 + 0.02×36
        Assert.Equal(clock.UtcNow, sink.Events[0].OccurredUtc);
        Assert.Equal(tenant, sink.Events[0].Tenant);
        Assert.Equal(rid, sink.Events[0].RequestId);
    }
}
