namespace Pochteca;

/// <summary>
/// Represents a declarative rule for unit calculation, supporting a base unit value and an optional per-item component.
/// The <see cref="EndpointPrefix"/> can match either an <see cref="EndpointKey"/> or a request path prefix.
/// </summary>
public sealed record UnitRule
{
	/// <summary>
	/// Initializes a new instance of the <see cref="UnitRule"/> record.
	/// </summary>
	/// <param name="endpointPrefix">The prefix to match against the endpoint key or request path. The longest matching prefix wins.</param>
	/// <param name="baseUnits">The base number of units to charge for a matching request.</param>
	/// <param name="perItemUnit">The number of units to add per item, if <paramref name="itemKey"/> is present in the request items.</param>
	/// <param name="itemKey">The key in the request items dictionary whose value determines the per-item count.</param>
	public UnitRule(
		string endpointPrefix,
		decimal baseUnits,
		decimal? perItemUnit = null,
		string? itemKey = null)
	{
		EndpointPrefix = endpointPrefix;
		BaseUnits = baseUnits;
		PerItemUnit = perItemUnit;
		ItemKey = itemKey;
	}

	/// <summary>
	/// Gets the prefix to match against the endpoint key or request path. The longest matching prefix wins.
	/// </summary>
	public string EndpointPrefix { get; }

	/// <summary>
	/// Gets the base number of units to charge for a matching request.
	/// </summary>
	public decimal BaseUnits { get; }

	/// <summary>
	/// Gets the number of units to add per item, if <see cref="ItemKey"/> is present in the request items.
	/// </summary>
	public decimal? PerItemUnit { get; }

	/// <summary>
	/// Gets the key in the request items dictionary whose value determines the per-item count.
	/// </summary>
	public string? ItemKey { get; }
}

/// <summary>
/// Defines a contract for resolving the best matching <see cref="UnitRule"/> for a given request.
/// </summary>
public interface IUnitRuleProvider
{
	/// <summary>
	/// Resolves the most appropriate <see cref="UnitRule"/> for the specified request.
	/// </summary>
	/// <param name="request">The request information to match against available rules.</param>
	/// <returns>
	/// The best matching <see cref="UnitRule"/>, or <c>null</c> if no rule matches.
	/// </returns>
	UnitRule? ResolveRule(RequestInfo request);
}