# Pochteca.Abstractions
Means "merchant" in Nahuatl, and is an abstractions package for billing and metering.

**Purpose:** Define contracts for usage metering, billable unit calculation, durable usage events, and idempotency/deduplication for billing pipelines.

## Primary Concepts

- **Payer**: the financially responsible party (tenant/org/user)
- **EndpointKey**: stable identifier for an API action (not the raw route)
- **Units**: decimal usage metric (fractional allowed)
- **UsageEvent**: immutable record emitted for aggregation
- **RequestId**: per-request correlation id (scoped to payer)
- **Idempotency**: dedupe for at-least-once pipelines

## Public Surface (v0.1)

- **Value Objects**: `TenantId`, `EndpointKey`, `IdempotencyKey`, `RequestId`
- **DTOs**: `RequestInfo`, `UsageEvent`, `UnitsResult`, `UnitRule`, `UsageBatchItem`
- **Interfaces**: `IUnitCalculator`, `IUsageSink`, `IUsageMeter`, `IUsageDedupeStore`, `IUnitRuleProvider`, `IClock`
- **Constants**: `HeaderNames`, `UsageCategories`, `EventSchema`

## Sample sequence (dedupe → calculate → event → write)

```csharp
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
    // 1) Dedupe (scoped by payer + endpoint + requestId + idempotencyKey)
    var isNew = await _dedupe.TryStampAsync(payer, request.Endpoint, requestId, idempotencyKey, TimeSpan.FromHours(24), cancellationToken);
    if (!isNew)
    {
        // Optional: surface the previously recorded event for exact echo/debug
        return await _dedupe.TryGetAsync(payer, request.Endpoint, requestId, idempotencyKey, cancellationToken);
    }

    // 2) Calculate units
    var units = _calculator.Calculate(request);

    // 3) Shape the event
    var @event = new UsageEvent(
        eventId: Guid.NewGuid(),
        payer: payer,
        endpoint: request.Endpoint,
        units: units.Units,
        occurredUtc: _clock.UtcNow,
        requestId: requestId,
        idempotencyKey: idempotencyKey,
        subject: subject,
        sponsor: sponsor,
        billable: billable,
        ruleId: units.RuleId,
        calcVersion: null,
        statusCode: request.StatusCode,
        metadata: null);

    // 4) Write to durable sink (batched)
    await _sink.WriteAsync(new[] { @event }, cancellationToken);

    return @event;
}
```
