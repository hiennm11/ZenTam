namespace ZenTam.Api.Features.EvaluateSpiritualAction.Requests;

using System;

/// <summary>
/// Wrapper request for evaluating a spiritual action.
/// Tier is auto-detected from which date fields are populated.
/// </summary>
public record EvaluateActionRequest
{
    public required Guid UserId { get; init; }
    public required string ActionCode { get; init; }

    // Tier detection - exactly ONE should be set:
    public int? TargetYear { get; init; }      // Year only
    public int? TargetMonth { get; init; }      // Year + Month (requires TargetYear)
    public int? TargetDay { get; init; }        // Lunar day (requires TargetMonth)
    public DateOnly? TargetDate { get; init; }  // Solar date → Day tier

    // Fallback: if no date fields → use current year, Year tier
}