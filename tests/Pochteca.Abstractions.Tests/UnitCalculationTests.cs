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
            new UnitRule("loans.v1", "/v1/loans", 1.00m, 0.02m, "periods"),
            new UnitRule("loans.amortize.v1", "/v1/loans/amortize", 1.10m, 0.02m, "periods") // more specific
        };

        var provider = new SimplePrefixUnitRuleProvider(rules);
        var calculator = new RuleBasedUnitCalculator(provider);

        var request = new RequestInfo(
            method: "POST",
            path: "/v1/loans/amortize",
            timestampUtc: DateTimeOffset.UtcNow,
            endpoint: new EndpointKey("Loans.Amortize"),
            statusCode: 200,
            items: new Dictionary<string, object?> { ["periods"] = 36 }
        );

        var result = calculator.Calculate(request);

        // Longest prefix wins → base 1.10 + (0.02 × 36) = 1.82
        Assert.Equal(1.82m, result.Units);
        Assert.Equal("loans.amortize.v1", result.RuleId);
    }
}
