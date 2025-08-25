namespace Pochteca;

/// <summary>
/// Defines a contract for computing billable units for a completed request.
/// </summary>
public interface IUnitCalculator
{
	/// <summary>
	/// Calculates the number of billable units for the specified request.
	/// </summary>
	/// <param name="request">The request information to use for unit calculation.</param>
	/// <returns>A <see cref="UnitsResult"/> containing the computed units and optional reason.</returns>
	UnitsResult Calculate(RequestInfo request);
}

/// <summary>
/// Defines a contract for persisting or enqueuing usage events in a durable and reliable manner.
/// </summary>
public interface IUsageSink
{
	/// <summary>
	/// Writes a batch of usage events to the underlying store or queue.
	/// </summary>
	/// <param name="batch">The batch of <see cref="UsageEvent"/> instances to write.</param>
	/// <param name="cancellationToken">A cancellation token for the operation.</param>
	/// <returns>A task representing the asynchronous write operation.</returns>
	Task WriteAsync(IReadOnlyList<UsageEvent> batch, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a facade for the full usage metering pipeline: calculate units, deduplicate, and write usage events.
/// </summary>
public interface IUsageMeter
{
	/// <summary>
	/// Attempts to record a usage event for the specified request, performing deduplication and unit calculation.
	/// </summary>
	/// <param name="tenant">The tenant for whom the usage is being recorded.</param>
	/// <param name="request">The request information.</param>
	/// <param name="idempotencyKey">The optional idempotency key for deduplication.</param>
	/// <param name="requestId">The unique request identifier.</param>
	/// <param name="cancellationToken">A cancellation token for the operation.</param>
	/// <returns>
	/// The <see cref="UsageEvent"/> if the event was recorded (first-seen), or <c>null</c> if it was a duplicate.
	/// </returns>
	Task<UsageEvent?> TryRecordAsync(
		TenantId tenant,
		RequestInfo request,
		IdempotencyKey? idempotencyKey,
		RequestId requestId,
		CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a contract for an at-least-once deduplication store, scoped by tenant.
/// </summary>
public interface IUsageDedupeStore
{
	/// <summary>
	/// Attempts to stamp a request as seen for deduplication purposes.
	/// </summary>
	/// <param name="tenant">The tenant scope for deduplication.</param>
	/// <param name="requestId">The unique request identifier.</param>
	/// <param name="idempotencyKey">The optional idempotency key.</param>
	/// <param name="ttl">The time-to-live for deduplication, after which the stamp expires.</param>
	/// <param name="cancellationToken">A cancellation token for the operation.</param>
	/// <returns>
	/// <c>true</c> if this stamp is new (first-seen); <c>false</c> if already seen and still within TTL.
	/// </returns>
	Task<bool> TryStampAsync(
		TenantId tenant,
		RequestId requestId,
		IdempotencyKey? idempotencyKey,
		TimeSpan ttl,
		CancellationToken cancellationToken = default);
}

/// <summary>
/// Provides an abstraction for retrieving the current UTC time, primarily for testability.
/// </summary>
public interface IClock
{
	/// <summary>
	/// Gets the current UTC time.
	/// </summary>
	DateTimeOffset UtcNow { get; }
}