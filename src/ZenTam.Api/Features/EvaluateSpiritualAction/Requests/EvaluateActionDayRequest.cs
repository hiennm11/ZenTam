namespace ZenTam.Api.Features.EvaluateSpiritualAction.Requests;

using System;

/// <summary>
/// Day-tier request for evaluating a spiritual action.
/// </summary>
public record EvaluateActionDayRequest
{
    public required Guid UserId { get; init; }
    public required string ActionCode { get; init; }
    public required DateOnly TargetDate { get; init; }  // Solar date
}