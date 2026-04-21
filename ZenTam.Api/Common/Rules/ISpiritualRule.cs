using ZenTam.Api.Common.Domain;

namespace ZenTam.Api.Common.Rules;

public interface ISpiritualRule
{
    string RuleCode { get; }
    RuleResult Evaluate(UserProfile profile, LunarDateContext context);
}
