namespace ZenTam.Api.Common.Rules;

using Common.Domain;
using Common.Rules.Models;
using Infrastructure.Entities;

public class RuleResolver
{
    private readonly Dictionary<string, ISpiritualRule> _registry;

    public RuleResolver(IEnumerable<ISpiritualRule> rules)
        => _registry = rules.ToDictionary(r => r.RuleCode);

    /// <summary>
    /// Resolve rule instances for the given action-rule mappings and user profile.
    /// Returns an immutable list of (Rule, IsMandatory) pairs.
    /// </summary>
    public IReadOnlyList<(ISpiritualRule Rule, bool IsMandatory)> Resolve(
        IEnumerable<ActionRuleMapping> mappings,
        Gender gender,
        RuleTier tier)
    {
        var targetScope = gender == Gender.Male ? GenderApplyScope.MaleOnly : GenderApplyScope.FemaleOnly;
        return mappings
            .Where(m => m.GenderScope == GenderApplyScope.Both || m.GenderScope == targetScope)
            .Where(m => m.Tier == RuleTier.All || m.Tier == tier)
            .Select(m =>
            {
                var rule = _registry.GetValueOrDefault(m.RuleCode);
                return (Rule: rule, IsMandatory: m.IsMandatory);
            })
            .Where(x => x.Rule != null)
            .Select(x => (x.Rule!, x.IsMandatory))
            .ToList();
    }
}
