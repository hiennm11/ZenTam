using ZenTam.Api.Common.Rules;
using ZenTam.Api.Domain.Services;

namespace ZenTam.Api.Features.EvaluateSpiritualAction.Queries;

public class EvaluateActionResponse
{
    public bool             IsAllowed  { get; init; }
    public int              TotalScore { get; init; }
    public string           Verdict    { get; set; } = string.Empty;
    public List<RuleResult> Details    { get; init; } = new();
    public GanhMenhResult?  GanhMenh   { get; set; }
}
