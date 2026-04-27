namespace ZenTam.Api.Features.EvaluateSpiritualAction.Models;

public record FindGoodDaysResponse(
    ActionCode Action,
    DateOnly SearchRangeStart,
    DateOnly SearchRangeEnd,
    int TotalDaysScanned,
    List<DayScoreResult> SuggestedDays
);