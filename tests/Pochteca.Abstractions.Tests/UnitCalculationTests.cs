using Pochteca;
using Tests.Fakes;
using Xunit;

public class UnitCalculationTests
{
    [Fact]
    public void Calculates_units_from_rule_with_per_item_component()
    {
        var rules = new[]
        {
            new UnitRule("/v1/loans", 1.00m, 0.02m, "periods"),
            new UnitRule("/v1/loans/amortize", 1.10m, 0.02m, "periods") // more specific
        };

        var provider = new SimplePrefixUnitRuleProvider(rules);
        var calc = new RuleBasedUnitCalculator(provider);

		var req = new RequestInfo(
			"POST",
			"/v1/loans/amortize",
			DateTimeOffset.UtcNow,
			new EndpointKey("Loans.Amortize"),
			200,
			new Dictionary<string, object?> { ["periods"] = 36 }
		);

        var result = calc.Calculate(req);

        // Longest prefix wins → base 1.10 + (0.02 × 36) = 1.82
        Assert.Equal(1.82m, result.Units);
        Assert.Null(result.Reason);
    }
}
