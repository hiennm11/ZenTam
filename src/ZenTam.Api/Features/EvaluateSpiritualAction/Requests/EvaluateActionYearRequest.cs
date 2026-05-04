namespace ZenTam.Api.Features.EvaluateSpiritualAction.Requests;

using System;

/// <summary>
/// Year-tier request for evaluating a spiritual action.
/// </summary>
public record EvaluateActionYearRequest
{
    public required Guid UserId { get; init; }
    public required string ActionCode { get; init; }
    public required int TargetYear { get; init; }
}