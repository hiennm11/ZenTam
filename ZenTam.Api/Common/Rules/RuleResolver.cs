using ZenTam.Api.Common.Domain;
using ZenTam.Api.Infrastructure.Entities;

namespace ZenTam.Api.Common.Rules;

public class RuleResolver
{
    private readonly Dictionary<string, ISpiritualRule> _registry;

    public RuleResolver(IEnumerable<ISpiritualRule> rules)
        => _registry = rules.ToDictionary(r => r.RuleCode);

    /// <summary>
    /// Returns only the rules applicable given the user's gender.
    /// A mapping with GenderConstraint == null applies to all genders.
    /// </summary>
    public IReadOnlyList<(ISpiritualRule Rule, bool IsMandatory)> Resolve(
        IEnumerable<ActionRuleMapping> mappings,
        Gender userGender)
    {
        return mappings
            .Where(m => m.GenderConstraint == null || m.GenderConstraint == userGender)
            .Where(m => _registry.ContainsKey(m.RuleCode))
            .Select(m => (_registry[m.RuleCode], m.IsMandatory))
            .ToList();
    }
}
