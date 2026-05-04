namespace ZenTam.Api.Features.EvaluateSpiritualAction.Responses;

/// <summary>
/// Month-tier response for action evaluation.
/// </summary>
public record EvaluateActionMonthResponse : EvaluateActionResponse
{
    public int TargetYear { get; init; }
    public int TargetMonth { get; init; }
    public int TargetDay { get; init; }
    public string LunarDateStr { get; init; }  // e.g., "14/3 Bính Ngọ"
}