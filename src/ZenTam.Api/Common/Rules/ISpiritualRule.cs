namespace ZenTam.Api.Common.Rules;

using Models;

/// <summary>
/// Unified spiritual rule interface for all lunar taboo evaluations.
/// All rules must implement this interface.
/// </summary>
public interface ISpiritualRule
{
    string RuleCode { get; }
    RuleEvaluation Evaluate(RuleContext context);
}
