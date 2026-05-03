namespace ZenTam.Api.Common.Rules;

using Models;

public class RuleResolverV2
{
    private readonly Dictionary<string, ISpiritualRule> _registry;

    public RuleResolverV2(IEnumerable<ISpiritualRule> rules)
        => _registry = rules.ToDictionary(r => r.RuleCode);

    public ISpiritualRule? Get(string ruleCode)
        => _registry.GetValueOrDefault(ruleCode);

    public IReadOnlyDictionary<string, ISpiritualRule> All => _registry;
}