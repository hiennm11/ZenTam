using ZenTam.Api.Common.Rules;
using ZenTam.Api.Common.Rules.Models;
using ZenTam.Api.Features.Calendars.Data;

namespace ZenTam.Api.Features.EvaluateSpiritualAction.Rules;

/// <summary>
/// <para><b>Sát Chủ V2</b> — "Day Sickness" taboo (ISpiritualRule).</para>
/// <para>
/// Sát Chủ là những ngày bị coi là xấu trong tháng âm lịch.
/// Mỗi tháng có một ngày Sát Chủ cố định.
/// </para>
/// <para><b>Algorithm:</b></para>
/// <para>
/// 1. satChuDay = SatChuLookup.GetSatChuDay(context.Lunar.LunarMonth)<br/>
/// 2. isSatChu = (context.Lunar.LunarDay == satChuDay)<br/>
/// 3. Fail → Severity=DaiHung, IsBlocked=true, ScoreImpact=-15, IsMandatory=true
/// </para>
/// </summary>
public class SatChuRuleV2 : ISpiritualRule
{
    public string RuleCode => "SAT_CHU";

    public RuleEvaluation Evaluate(RuleContext context)
    {
        bool isSatChu = SatChuLookup.IsSatChu(context.Lunar.LunarMonth, context.Lunar.LunarDay);
        int satChuDay = SatChuLookup.GetSatChuDay(context.Lunar.LunarMonth);

        return new RuleEvaluation
        {
            RuleCode = RuleCode,
            IsPassed = !isSatChu,
            ScoreImpact = isSatChu ? -15 : 0,
            Severity = isSatChu ? RuleSeverity.DaiHung : RuleSeverity.None,
            IsBlocked = isSatChu,
            IsMandatory = isSatChu,
            Message = isSatChu
                ? $"Sát Chủ: Ngày mùng {satChuDay} tháng {context.Lunar.LunarMonth} — Xấu"
                : $"Không Sát Chủ (ngày {context.Lunar.LunarDay}/{context.Lunar.LunarMonth})"
        };
    }
}
