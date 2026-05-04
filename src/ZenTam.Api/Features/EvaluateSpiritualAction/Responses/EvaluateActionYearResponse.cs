namespace ZenTam.Api.Features.EvaluateSpiritualAction.Responses;

/// <summary>
/// Year-tier response for action evaluation.
/// </summary>
public record EvaluateActionYearResponse : EvaluateActionResponse
{
    public int TargetYear { get; init; }
    public int ClientAge { get; init; }       // Lunar age (tuổi âm)
    public string CanChiNam { get; init; }    // e.g., "Bính Ngọ"
}