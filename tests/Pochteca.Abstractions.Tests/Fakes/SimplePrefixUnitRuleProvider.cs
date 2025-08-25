using Pochteca;

namespace Tests.Fakes;

internal sealed class SimplePrefixUnitRuleProvider : IUnitRuleProvider
{
    private readonly IReadOnlyList<UnitRule> _rules;

    public SimplePrefixUnitRuleProvider(IEnumerable<UnitRule> rules)
        => _rules = rules.ToList();

    public UnitRule? ResolveRule(RequestInfo request)
    {
        string key = request.Endpoint.Value ?? string.Empty;
        string path = request.Path ?? string.Empty;

        // Longest matching prefix wins (EndpointKey preferred, else Path)
        UnitRule? best = null;
        int bestLen = -1;

        foreach (var r in _rules)
        {
            if (IsPrefix(key, r.EndpointPrefix) && r.EndpointPrefix.Length > bestLen)
            {
                best = r; bestLen = r.EndpointPrefix.Length;
            }
        }

        if (best is not null) return best;

        foreach (var r in _rules)
        {
            if (IsPrefix(path, r.EndpointPrefix) && r.EndpointPrefix.Length > bestLen)
            {
                best = r; bestLen = r.EndpointPrefix.Length;
            }
        }

        return best;

        static bool IsPrefix(string text, string prefix)
            => !string.IsNullOrEmpty(prefix) && text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    }
}
