# Pochteca.Abstractions
Means "merchant" in Nahuatl, and is an abstractions package for billing and metering.

**Purpose:** Define contracts for usage metering, billable unit calculation, durable usage events, and idempotency/deduplication for billing pipelines.

## Primary Concepts

- **Tenant**: the billing subject (org or user)
- **EndpointKey**: stable identifier for an API action (not the raw route)
- **Units**: decimal usage metric (fractional allowed)
- **UsageEvent**: immutable record emitted for aggregation
- **RequestId**: per-request correlation id (scoped to tenant)
- **Idempotency**: dedupe for at‑least‑once pipelines

## Public Surface (v0.1)

- **Value Objects**: `TenantId`, `EndpointKey`, `IdempotencyKey`, `RequestId`
- **DTOs**: `RequestInfo`, `UsageEvent`, `UnitsResult`, `UnitRule`
- **Interfaces**: `IUnitCalculator`, `IUsageSink`, `IUsageMeter`, `IUsageDedupeStore`, `IUnitRuleProvider`, `IClock`
- **Constants**: `HeaderNames`

## Sample sequence (dedupe → calculate → event → write)

```csharp
public async Task<UsageEvent?> TryRecordAsync(
    TenantId tenant,
    RequestInfo request,
    IdempotencyKey? idempotencyKey,
    RequestId requestId,
    CancellationToken ct)
{
    // 1) Dedupe stamp (scoped to Tenant + RequestId + IdempotencyKey)
    var ttl = TimeSpan.FromHours(24); // implementation decides window
    var isNew = await _dedupe.TryStampAsync(tenant, requestId, idempotencyKey, ttl, ct);
    if (!isNew) return null;

    // 2) Calculate units
    var units = _calculator.Calculate(request);

    // 3) Shape the usage event
    var evt = new UsageEvent(
        EventId: Guid.NewGuid(),
        Tenant: tenant,
        Endpoint: request.Endpoint,
        Units: units.Units,
        OccurredUtc: _clock.UtcNow,
        RequestId: requestId,
        IdempotencyKey: idempotencyKey,
        Metadata: null
    );

    // 4) Write to durable sink
    await _sink.WriteAsync(new[] { evt }, ct);

    return evt;
}
