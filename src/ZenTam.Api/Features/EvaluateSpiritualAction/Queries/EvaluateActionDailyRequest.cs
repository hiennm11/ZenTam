namespace ZenTam.Api.Features.EvaluateSpiritualAction.Queries;

/// <summary>
/// Request for evaluating a spiritual action on a specific day.
/// </summary>
public class EvaluateActionDailyRequest
{
    /// <summary>
    /// The user's profile ID.
    /// </summary>
    public Guid? UserId { get; init; }

    /// <summary>
    /// The action code to evaluate (e.g., "XAY_NHA", "MAU_TOC").
    /// </summary>
    public string ActionCode { get; init; } = string.Empty;

    /// <summary>
    /// The target solar date to evaluate.
    /// </summary>
    public DateOnly TargetDate { get; init; }
}