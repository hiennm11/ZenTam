namespace ZenTam.Api.Features.EvaluateSpiritualAction.Requests;

/// <summary>
/// Month-tier request for evaluating a spiritual action.
/// </summary>
public record EvaluateActionMonthRequest
{
    public required Guid UserId { get; init; }
    public required string ActionCode { get; init; }
    public required int TargetYear { get; init; }
    public required int TargetMonth { get; init; }  // 1-12
    public required int TargetDay { get; init; }    // Lunar day 1-30
}