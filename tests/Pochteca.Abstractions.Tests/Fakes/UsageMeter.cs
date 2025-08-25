using Pochteca;

namespace Tests.Fakes;

/// <summary>Reference implementation for tests only.</summary>
internal sealed class UsageMeter : IUsageMeter
{
    private readonly IUnitCalculator _calculator;
    private readonly IUsageDedupeStore _dedupe;
    private readonly IUsageSink _sink;
    private readonly IClock _clock;
    private readonly TimeSpan _ttl;

    public UsageMeter(IUnitCalculator calculator, IUsageDedupeStore dedupe, IUsageSink sink, IClock clock, TimeSpan ttl)
    {
        _calculator = calculator;
        _dedupe = dedupe;
        _sink = sink;
        _clock = clock;
        _ttl = ttl;
    }

    public async Task<UsageEvent?> TryRecordAsync(
        TenantId tenant,
        RequestInfo request,
        IdempotencyKey? idempotencyKey,
        RequestId requestId,
        CancellationToken ct = default)
    {
        // Dedupe first to avoid unnecessary work on obvious dupes.
        var isNew = await _dedupe.TryStampAsync(tenant, requestId, idempotencyKey, _ttl, ct).ConfigureAwait(false);
        if (!isNew) return null;

        var unitsResult = _calculator.Calculate(request);
		var evt = new UsageEvent(
			Guid.NewGuid(), // EventId
			tenant,
			request.Endpoint,
			unitsResult.Units,
			_clock.UtcNow,
			requestId,
			idempotencyKey,
			null // Metadata
		);

        await _sink.WriteAsync(new[] { evt }, ct).ConfigureAwait(false);
        return evt;
    }
}
