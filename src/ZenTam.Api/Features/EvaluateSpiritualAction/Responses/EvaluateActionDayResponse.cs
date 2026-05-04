namespace ZenTam.Api.Features.EvaluateSpiritualAction.Responses;

using System;

/// <summary>
/// Day-tier response for action evaluation.
/// </summary>
public record EvaluateActionDayResponse : EvaluateActionResponse
{
    public DateOnly TargetDate { get; init; }
    public string LunarDateStr { get; init; }   // e.g., "14/3 Bính Ngọ"
    public string CanChiNgay { get; init; }    // e.g., "Giáp Tý"
    public string TrucNgay { get; init; }      // e.g., "Thành"
}