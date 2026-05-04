namespace ZenTam.Api.Features.EvaluateSpiritualAction.Queries;

public class EvaluateActionRequest
{
    public Guid?   UserId     { get; init; }
    public string ActionCode { get; init; } = string.Empty;
    public int    TargetYear { get; init; }
}
