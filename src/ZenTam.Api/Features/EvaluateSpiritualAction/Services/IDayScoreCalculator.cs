using ZenTam.Api.Common.Domain;
using ZenTam.Api.Features.EvaluateSpiritualAction.Models;

namespace ZenTam.Api.Features.EvaluateSpiritualAction.Services;

public interface IDayScoreCalculator
{
    DayScoreResult Calculate(DateTime solarDate, ActionCode action, Gender userGender, RuleTier tier, int? clientLunarYear = null);
}