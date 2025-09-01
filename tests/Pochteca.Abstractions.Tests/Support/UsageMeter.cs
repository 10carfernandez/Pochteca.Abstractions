using Pochteca;

namespace Tests.Fakes;

/// <summary>Reference implementation for tests only.</summary>
internal sealed class UsageMeter : IUsageMeter
{
    private readonly IUnitCalculator _calculator;
    private readonly IUsageDedupeStore _dedupeStore;
    private readonly IUsageSink _sink;
    private readonly IClock _clock;
    private readonly TimeSpan _timeToLive;

    public UsageMeter(IUnitCalculator calculator, IUsageDedupeStore dedupeStore, IUsageSink sink, IClock clock, TimeSpan timeToLive)
    {
        _calculator = calculator;
        _dedupeStore = dedupeStore;
        _sink = sink;
        _clock = clock;
        _timeToLive = timeToLive;
    }

    public async Task<UsageEvent?> TryRecordAsync(
        TenantId payer,
        RequestInfo request,
        IdempotencyKey? idempotencyKey,
        RequestId requestId,
        bool billable = true,
        TenantId? subject = null,
        string? sponsor = null,
        CancellationToken cancellationToken = default)
    {
        // Dedupe first to avoid unnecessary work on obvious dupes.
        var isNew = await _dedupeStore.TryStampAsync(payer, request.Endpoint, requestId, idempotencyKey, _timeToLive, cancellationToken).ConfigureAwait(false);
        if (!isNew)
        {
            // This fake meter returns null on duplicates (even if a prior event existed).
            return null;
        }

        var unitsResult = _calculator.Calculate(request);
        var usageEvent = new UsageEvent(
            eventId: Guid.NewGuid(),
            payer: payer,
            endpoint: request.Endpoint,
            units: unitsResult.Units,
            occurredUtc: _clock.UtcNow,
            requestId: requestId,
            idempotencyKey: idempotencyKey,
            subject: subject,
            sponsor: sponsor,
            billable: billable,
            ruleId: unitsResult.RuleId,
            calcVersion: null,
            statusCode: request.StatusCode,
            metadata: null
        );

        await _sink.WriteAsync(new[] { usageEvent }, cancellationToken).ConfigureAwait(false);
        return usageEvent;
    }

    public async Task<IReadOnlyList<UsageEvent>> TryRecordBatchAsync(
        TenantId payer,
        IReadOnlyList<UsageBatchItem> batch,
        CancellationToken cancellationToken = default)
    {
        var results = new List<UsageEvent>(batch.Count);
        var toWrite = new List<UsageEvent>();

        foreach (var item in batch)
        {
            var isNew = await _dedupeStore.TryStampAsync(payer, item.Request.Endpoint, item.RequestId, item.IdempotencyKey, _timeToLive, cancellationToken).ConfigureAwait(false);
            if (!isNew)
            {
                // Do not include duplicates in this simple fake; real impls may return the prior event.
                continue;
            }

            var unitsResult = _calculator.Calculate(item.Request);
            var usageEvent = new UsageEvent(
                eventId: Guid.NewGuid(),
                payer: payer,
                endpoint: item.Request.Endpoint,
                units: unitsResult.Units,
                occurredUtc: _clock.UtcNow,
                requestId: item.RequestId,
                idempotencyKey: item.IdempotencyKey,
                subject: item.Subject,
                sponsor: item.Sponsor,
                billable: item.Billable,
                ruleId: unitsResult.RuleId,
                calcVersion: null,
                statusCode: item.Request.StatusCode,
                metadata: null
            );

            toWrite.Add(usageEvent);
            results.Add(usageEvent);
        }

        if (toWrite.Count > 0)
        {
            await _sink.WriteAsync(toWrite, cancellationToken).ConfigureAwait(false);
        }

        return results;
    }
}
