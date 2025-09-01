using Pochteca;
using Tests.Fakes;
using Xunit;

public class UsageMeterIntegrationTests
{
    [Fact]
    public async Task TryRecord_calculates_dedupes_and_writes_once()
    {
        var rules = new[] { new UnitRule("loans.v1", "/v1/loans", 1.00m, 0.02m, "periods") };
        var calculator = new RuleBasedUnitCalculator(new SimplePrefixUnitRuleProvider(rules));

        var clock = new FixedClock(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero));
        var dedupeStore = new InMemoryDedupeStore(clock);
        var sink = new CollectingUsageSink();
        var meter = new UsageMeter(calculator, dedupeStore, sink, clock, timeToLive: TimeSpan.FromHours(24));

        var payer = new TenantId(Guid.NewGuid());
        var requestId = new RequestId("req-123");
        var idempotencyKey = new IdempotencyKey("idem-abc");
        var request = new RequestInfo(
            method: "POST",
            path: "/v1/loans/amortize",
            timestampUtc: clock.UtcNow,
            endpoint: new EndpointKey("Loans.Amortize"),
            statusCode: 200,
            items: new Dictionary<string, object?> { ["periods"] = 36 });

        var first = await meter.TryRecordAsync(payer, request, idempotencyKey, requestId);
        var second = await meter.TryRecordAsync(payer, request, idempotencyKey, requestId);

        Assert.NotNull(first);
        Assert.Null(second); // deduped in this simple fake
        Assert.Single(sink.Events);
        Assert.Equal(1.72m, sink.Events[0].Units); // 1 + 0.02×36
        Assert.Equal(clock.UtcNow, sink.Events[0].OccurredUtc);
        Assert.Equal(payer, sink.Events[0].Payer);
        Assert.Equal(requestId, sink.Events[0].RequestId);
    }
}
