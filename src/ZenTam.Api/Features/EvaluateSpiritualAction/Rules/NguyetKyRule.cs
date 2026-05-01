using ZenTam.Api.Common.Domain;
using ZenTam.Api.Common.Rules;

namespace ZenTam.Api.Features.EvaluateSpiritualAction.Rules;

/// <summary>
/// <para><b>Nguyệt Kỵ</b> — "Monthly Taboo" days.</para>
/// <para>
/// Một số ngày âm lịch bị coi là xấu không phân biệt tháng.
/// Ngày mùng 5, 14, 23 hàng tháng là những ngày "Nguyệt Kỵ" -
/// kiêng làm việc lớn, kiêng di chuyển xa.
/// </para>
/// <para><b>Algorithm:</b></para>
/// <para>
/// 1. Get LunarDay from LunarDateContext<br/>
/// 2. Fail when LunarDay ∈ {5, 14, 23}<br/>
/// 3. Score on fail: −10 (not mandatory)
/// </para>
/// </summary>
public class NguyetKyRule : ISpiritualRule
{
    public string RuleCode => "NGUYET_KY";

    private static readonly int[] TabooDays = { 5, 14, 23 };

    public RuleResult Evaluate(UserProfile profile, LunarDateContext context)
    {
        bool isPassed = !TabooDays.Contains(context.LunarDay);

        return new RuleResult
        {
            RuleName = RuleCode,
            IsPassed = isPassed,
            Score    = isPassed ? 0 : -10,
            Message  = isPassed
                ? $"Không phạm Nguyệt Kỵ (ngày {context.LunarDay})"
                : $"Phạm Nguyệt Kỵ (mùng {context.LunarDay}) — ngày kiêng kỵ hàng tháng"
        };
    }
}