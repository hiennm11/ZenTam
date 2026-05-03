using ZenTam.Api.Common.Rules;
using ZenTam.Api.Common.Rules.Models;

namespace ZenTam.Api.Features.EvaluateSpiritualAction.Rules;

/// <summary>
/// <para><b>Tam Nuông V2</b> — "Three Fires" taboo (ISpiritualRule).</para>
/// <para>
/// Tam Nuông là kiêng kỵ liên quan đến lửa và nghề nghiệp.
/// Các ngày này bị coi là xấu cho việc thi công, xây dựng,
/// hoặc bắt đầu công việc quan trọng.
/// </para>
/// <para><b>Algorithm:</b></para>
/// <para>
/// 1. Get LunarDay from RuleContext.Lunar.LunarDay<br/>
/// 2. Fail when LunarDay ∈ {3, 7, 13, 18, 22, 27}<br/>
/// 3. Fail → Severity=Hung, IsBlocked=true, ScoreImpact=-8
/// </para>
/// </summary>
public class TamNuongRuleV2 : ISpiritualRule
{
    public string RuleCode => "TAM_NUONG";

    private static readonly int[] TamNuongDays = { 3, 7, 13, 18, 22, 27 };

    public RuleEvaluation Evaluate(RuleContext context)
    {
        bool isPassed = !TamNuongDays.Contains(context.Lunar.LunarDay);

        return new RuleEvaluation
        {
            RuleCode = RuleCode,
            IsPassed = isPassed,
            ScoreImpact = isPassed ? 0 : -8,
            Severity = isPassed ? RuleSeverity.None : RuleSeverity.Hung,
            IsBlocked = !isPassed,
            IsMandatory = !isPassed,
            Message = isPassed
                ? $"Không phạm Tam Nuông (ngày {context.Lunar.LunarDay})"
                : $"Phạm Tam Nuông (mùng {context.Lunar.LunarDay}) — ngày kiêng lửa"
        };
    }
}
