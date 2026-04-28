using ZenTam.Api.Common.Domain;
using ZenTam.Api.Infrastructure.Entities;

namespace ZenTam.Api.Common.Rules;

public class RuleResolver
{
    private readonly Dictionary<string, ISpiritualRule> _registry;

    public RuleResolver(IEnumerable<ISpiritualRule> rules)
        => _registry = rules.ToDictionary(r => r.RuleCode);

    /// <summary>
    /// Returns only the rules applicable given the user's gender and tier.
    /// A mapping with GenderScope == Both applies to all genders.
    /// Tier must match requestedTier or be All.
    /// </summary>
    public IReadOnlyList<(ISpiritualRule Rule, bool IsMandatory)> Resolve(
        IEnumerable<ActionRuleMapping> mappings,
        Gender userGender,
        RuleTier requestedTier)
    {
        return mappings
            .Where(m => m.GenderScope == GenderApplyScope.Both 
                     || (m.GenderScope == GenderApplyScope.MaleOnly && userGender == Gender.Male)
                     || (m.GenderScope == GenderApplyScope.FemaleOnly && userGender == Gender.Female))
            .Where(m => m.Tier == requestedTier || m.Tier == RuleTier.All)
            .Where(m => _registry.ContainsKey(m.RuleCode))
            .Select(m => (_registry[m.RuleCode], m.IsMandatory))
            .ToList();
    }

    /// <summary>
    /// Returns only the rules applicable given the user's gender.
    /// A mapping with GenderScope == Both applies to all genders.
    /// </summary>
    public IReadOnlyList<(ISpiritualRule Rule, bool IsMandatory)> ResolveLegacy(
        IEnumerable<ActionRuleMapping> mappings,
        Gender userGender)
    {
        return mappings
            .Where(m => m.GenderScope == GenderApplyScope.Both 
                     || (m.GenderScope == GenderApplyScope.MaleOnly && userGender == Gender.Male)
                     || (m.GenderScope == GenderApplyScope.FemaleOnly && userGender == Gender.Female))
            .Where(m => _registry.ContainsKey(m.RuleCode))
            .Select(m => (_registry[m.RuleCode], m.IsMandatory))
            .ToList();
    }
}
