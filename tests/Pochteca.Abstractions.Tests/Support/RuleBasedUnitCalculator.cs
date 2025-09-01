using Pochteca;

namespace Tests.Fakes;

internal sealed class RuleBasedUnitCalculator : IUnitCalculator
{
    private readonly IUnitRuleProvider _provider;

    public RuleBasedUnitCalculator(IUnitRuleProvider provider) => _provider = provider;

    public UnitsResult Calculate(RequestInfo request)
    {
        var rule = _provider.ResolveRule(request);
        if (rule is null)
            return new UnitsResult(0m, "no_match", null);

        decimal units = rule.BaseUnits;

        if (rule.PerItemUnit is { } perItem && !string.IsNullOrWhiteSpace(rule.ItemKey)
            && request.Items.TryGetValue(rule.ItemKey!, out var raw) && TryToDecimal(raw, out var quantity))
        {
            units += perItem * quantity;
        }

        return new UnitsResult(units, $"rule:{rule.Id}", rule.Id);

        static bool TryToDecimal(object? value, out decimal result)
        {
            switch (value)
            {
                case null: result = 0; return false;
                case decimal d: result = d; return true;
                case int i: result = i; return true;
                case long l: result = l; return true;
                case double dbl: result = (decimal)dbl; return true;
                case float f: result = (decimal)f; return true;
                case string s when decimal.TryParse(s, out var parsed): result = parsed; return true;
                default: result = 0; return false;
            }
        }
    }
}
