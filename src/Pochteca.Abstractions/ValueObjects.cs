namespace Pochteca;

/// <summary>
/// Represents a stable, globally unique identifier for a tenant.
/// This value is intended to be consistent across distributed systems and services.
/// </summary>
public readonly record struct TenantId
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TenantId"/> struct.
	/// </summary>
	/// <param name="value">The globally unique identifier for the tenant.</param>
	public TenantId(Guid value) => Value = value;

	/// <summary>
	/// Gets the globally unique identifier for the tenant.
	/// </summary>
	public Guid Value { get; }

	/// <summary>
	/// Returns the string representation of the underlying <see cref="Value"/>.
	/// </summary>
	/// <returns>The string representation of the tenant ID.</returns>
	public override string ToString() => Value.ToString();
}

/// <summary>
/// Represents a stable identifier for an API action or endpoint.
/// This is not necessarily the raw route, but a logical key for billing and metering.
/// </summary>
public readonly record struct EndpointKey
{
	/// <summary>
	/// Initializes a new instance of the <see cref="EndpointKey"/> struct.
	/// </summary>
	/// <param name="value">The logical key representing the API endpoint or action.</param>
	public EndpointKey(string value) => Value = value;

	/// <summary>
	/// Gets the logical key representing the API endpoint or action.
	/// </summary>
	public string Value { get; }

	/// <summary>
	/// Returns the string representation of the endpoint key.
	/// </summary>
	/// <returns>The string representation of the endpoint key.</returns>
	public override string ToString() => Value;
}

/// <summary>
/// Represents an idempotency key supplied by clients to prevent duplicate billing or event processing.
/// </summary>
public readonly record struct IdempotencyKey
{
	/// <summary>
	/// Initializes a new instance of the <see cref="IdempotencyKey"/> struct.
	/// </summary>
	/// <param name="value">The idempotency key provided by the client.</param>
	public IdempotencyKey(string value) => Value = value;

	/// <summary>
	/// Gets the idempotency key provided by the client.
	/// </summary>
	public string Value { get; }

	/// <summary>
	/// Returns the string representation of the idempotency key.
	/// </summary>
	/// <returns>The string representation of the idempotency key.</returns>
	public override string ToString() => Value;
}

/// <summary>
/// Represents a request-scoped identifier, expected to be unique per tenant.
/// Used for deduplication and traceability.
/// </summary>
public readonly record struct RequestId
{
	/// <summary>
	/// Initializes a new instance of the <see cref="RequestId"/> struct.
	/// </summary>
	/// <param name="value">The unique identifier for the request.</param>
	public RequestId(string value) => Value = value;

	/// <summary>
	/// Gets the unique identifier for the request.
	/// </summary>
	public string Value { get; }

	/// <summary>
	/// Returns the string representation of the request identifier.
	/// </summary>
	/// <returns>The string representation of the request ID.</returns>
	public override string ToString() => Value;
}