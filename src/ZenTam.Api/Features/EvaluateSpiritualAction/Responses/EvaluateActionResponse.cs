namespace ZenTam.Api.Features.EvaluateSpiritualAction.Responses;

using System.Collections.Generic;
using ZenTam.Api.Common.Rules;
using ZenTam.Api.Domain.Services;

#pragma warning disable CS8618 // Non-nullable properties initialized by framework via init

/// <summary>
/// Base response for action evaluation at any tier.
/// </summary>
public record EvaluateActionResponse
{
    public bool IsAllowed { get; init; }
    public int TotalScore { get; init; }
    public string Verdict { get; init; } = "AN_TOAN";   // "AN_TOAN", "CANH_BAO", "CAM"
    public string TierUsed { get; init; } = "Year";     // "Year", "Month", "Day"
    public List<RuleResult> Details { get; init; } = new();
    public GanhMenhResult? GanhMenh { get; init; }      // null for Day tier
}

#pragma warning restore CS8618