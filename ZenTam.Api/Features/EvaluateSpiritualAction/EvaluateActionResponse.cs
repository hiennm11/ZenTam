using ZenTam.Api.Common.Rules;

namespace ZenTam.Api.Features.EvaluateSpiritualAction;

public class EvaluateActionResponse
{
    public bool             IsAllowed  { get; init; }
    public int              TotalScore { get; init; }
    public string           Verdict    { get; init; } = string.Empty;
    public List<RuleResult> Details    { get; init; } = new();
}
