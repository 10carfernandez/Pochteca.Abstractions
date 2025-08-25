namespace Pochteca;

/// <summary>
/// Provides constant HTTP header names used to carry idempotency and usage metering hints between clients and APIs.
/// </summary>
public static class HeaderNames
{
	/// <summary>
	/// The HTTP header name for the idempotency key, used to prevent duplicate billing or event processing.
	/// </summary>
	public const string IdempotencyKey = "Idempotency-Key";

	/// <summary>
	/// The HTTP header name for the request identifier, used for deduplication and traceability.
	/// </summary>
	public const string RequestId = "X-Request-Id";

	/// <summary>
	/// The HTTP header name for echoing the computed usage units in the API response (optional).
	/// </summary>
	public const string UsageUnits = "X-Usage-Units";
}
