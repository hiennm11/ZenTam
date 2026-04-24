namespace ZenTam.Api.Features.EvaluateSpiritualAction;

public class EvaluateActionRequest
{
    public Guid   UserId     { get; init; }
    public string ActionCode { get; init; } = string.Empty;
    public int    TargetYear { get; init; }
}
