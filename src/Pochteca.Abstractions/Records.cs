namespace Pochteca;

/// <summary>
/// Contains the minimal set of facts about a request needed for unit calculation and event shaping.
/// </summary>
public sealed record RequestInfo
{
	/// <summary>
	/// Initializes a new instance of the <see cref="RequestInfo"/> record.
	/// </summary>
	/// <param name="method">The HTTP method (e.g., GET, POST) of the request.</param>
	/// <param name="path">The raw path of the request, as received by the API.</param>
	/// <param name="timestampUtc">The UTC timestamp when the request was received or processed.</param>
	/// <param name="endpoint">A stable logical key representing the API endpoint or action.</param>
	/// <param name="statusCode">The HTTP status code returned for the request.</param>
	/// <param name="items">A dictionary of additional request-scoped items, such as counts or custom values, used for unit calculation.</param>
	public RequestInfo(
		string method,
		string path,
		DateTimeOffset timestampUtc,
		EndpointKey endpoint,
		int statusCode,
		IReadOnlyDictionary<string, object?> items)
	{
		Method = method;
		Path = path;
		TimestampUtc = timestampUtc;
		Endpoint = endpoint;
		StatusCode = statusCode;
		Items = items;
	}

	/// <summary>
	/// Gets the HTTP method (e.g., GET, POST) of the request.
	/// </summary>
	public string Method { get; }

	/// <summary>
	/// Gets the raw path of the request, as received by the API.
	/// </summary>
	public string Path { get; }

	/// <summary>
	/// Gets the UTC timestamp when the request was received or processed.
	/// </summary>
	public DateTimeOffset TimestampUtc { get; }

	/// <summary>
	/// Gets the stable logical key representing the API endpoint or action.
	/// </summary>
	public EndpointKey Endpoint { get; }

	/// <summary>
	/// Gets the HTTP status code returned for the request.
	/// </summary>
	public int StatusCode { get; }

	/// <summary>
	/// Gets the dictionary of additional request-scoped items, such as counts or custom values, used for unit calculation.
	/// </summary>
	public IReadOnlyDictionary<string, object?> Items { get; }
}

/// <summary>
/// Represents an immutable usage event that can be persisted or aggregated by billing pipelines.
/// </summary>
public sealed record UsageEvent
{
	/// <summary>
	/// Initializes a new instance of the <see cref="UsageEvent"/> record.
	/// </summary>
	/// <param name="eventId">A unique identifier for this usage event.</param>
	/// <param name="tenant">The tenant associated with this usage event.</param>
	/// <param name="endpoint">The logical endpoint or action for which usage is being recorded.</param>
	/// <param name="units">The number of billable units computed for this event.</param>
	/// <param name="occurredUtc">The UTC timestamp when the event occurred.</param>
	/// <param name="requestId">The unique request identifier for deduplication and traceability.</param>
	/// <param name="idempotencyKey">The optional idempotency key provided by the client, if any.</param>
	/// <param name="metadata">Optional metadata as key-value pairs for downstream enrichment or auditing.</param>
	public UsageEvent(
		Guid eventId,
		TenantId tenant,
		EndpointKey endpoint,
		decimal units,
		DateTimeOffset occurredUtc,
		RequestId requestId,
		IdempotencyKey? idempotencyKey,
		IReadOnlyDictionary<string, string>? metadata = null)
	{
		EventId = eventId;
		Tenant = tenant;
		Endpoint = endpoint;
		Units = units;
		OccurredUtc = occurredUtc;
		RequestId = requestId;
		IdempotencyKey = idempotencyKey;
		Metadata = metadata;
	}

	/// <summary>
	/// Gets the unique identifier for this usage event.
	/// </summary>
	public Guid EventId { get; }

	/// <summary>
	/// Gets the tenant associated with this usage event.
	/// </summary>
	public TenantId Tenant { get; }

	/// <summary>
	/// Gets the logical endpoint or action for which usage is being recorded.
	/// </summary>
	public EndpointKey Endpoint { get; }

	/// <summary>
	/// Gets the number of billable units computed for this event.
	/// </summary>
	public decimal Units { get; }

	/// <summary>
	/// Gets the UTC timestamp when the event occurred.
	/// </summary>
	public DateTimeOffset OccurredUtc { get; }

	/// <summary>
	/// Gets the unique request identifier for deduplication and traceability.
	/// </summary>
	public RequestId RequestId { get; }

	/// <summary>
	/// Gets the optional idempotency key provided by the client, if any.
	/// </summary>
	public IdempotencyKey? IdempotencyKey { get; }

	/// <summary>
	/// Gets the optional metadata as key-value pairs for downstream enrichment or auditing.
	/// </summary>
	public IReadOnlyDictionary<string, string>? Metadata { get; }
}

/// <summary>
/// Represents the result of a unit calculation for a request, including the computed units and an optional human-readable reason.
/// </summary>
public sealed record UnitsResult
{
	/// <summary>
	/// Initializes a new instance of the <see cref="UnitsResult"/> record.
	/// </summary>
	/// <param name="units">The number of units computed for the request.</param>
	/// <param name="reason">An optional explanation or reason for the computed units, useful for diagnostics or transparency.</param>
	public UnitsResult(decimal units, string? reason = null)
	{
		Units = units;
		Reason = reason;
	}

	/// <summary>
	/// Gets the number of units computed for the request.
	/// </summary>
	public decimal Units { get; }

	/// <summary>
	/// Gets an optional explanation or reason for the computed units, useful for diagnostics or transparency.
	/// </summary>
	public string? Reason { get; }
}