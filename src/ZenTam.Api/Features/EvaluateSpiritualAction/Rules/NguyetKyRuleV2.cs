using ZenTam.Api.Common.Rules;
using ZenTam.Api.Common.Rules.Models;

namespace ZenTam.Api.Features.EvaluateSpiritualAction.Rules;

/// <summary>
/// <para><b>Nguyệt Kỵ V2</b> — "Monthly Taboo" days (ISpiritualRule).</para>
/// <para>
/// Một số ngày âm lịch bị coi là xấu không phân biệt tháng.
/// Ngày mùng 5, 14, 23 hàng tháng là những ngày "Nguyệt Kỵ" -
/// kiêng làm việc lớn, kiêng di chuyển xa.
/// </para>
/// <para><b>Algorithm:</b></para>
/// <para>
/// 1. Get LunarDay from RuleContext.Lunar.LunarDay<br/>
/// 2. Fail when LunarDay ∈ {5, 14, 23}<br/>
/// 3. Fail → Severity=DaiHung, IsBlocked=true, ScoreImpact=-10
/// </para>
/// </summary>
public class NguyetKyRuleV2 : ISpiritualRule
{
    public string RuleCode => "NGUYET_KY";

    private static readonly int[] TabooDays = { 5, 14, 23 };

    public RuleEvaluation Evaluate(RuleContext context)
    {
        bool isPassed = !TabooDays.Contains(context.Lunar.LunarDay);

        return new RuleEvaluation
        {
            RuleCode = RuleCode,
            IsPassed = isPassed,
            ScoreImpact = isPassed ? 0 : -10,
            Severity = isPassed ? RuleSeverity.None : RuleSeverity.DaiHung,
            IsBlocked = !isPassed,
            IsMandatory = !isPassed,
            Message = isPassed
                ? $"Không phạm Nguyệt Kỵ (ngày {context.Lunar.LunarDay})"
                : $"Phạm Nguyệt Kỵ (mùng {context.Lunar.LunarDay}) — ngày kiêng kỵ hàng tháng"
        };
    }
}
