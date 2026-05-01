using ZenTam.Api.Common.Domain;
using ZenTam.Api.Common.Rules;

namespace ZenTam.Api.Features.EvaluateSpiritualAction.Rules;

/// <summary>
/// <para><b>Tam Nuông</b> — "Three Fires" taboo.</para>
/// <para>
/// Tam Nuông là kiêng kỵ liên quan đến lửa và nghề nghiệp.
/// Các ngày này bị coi là xấu cho việc thi công, xây dựng,
/// hoặc bắt đầu công việc quan trọng.
/// </para>
/// <para><b>Algorithm:</b></para>
/// <para>
/// 1. Get LunarDay from LunarDateContext<br/>
/// 2. Fail when LunarDay ∈ {3, 7, 13, 18, 22, 27}<br/>
/// 3. Score on fail: −8 (not mandatory)
/// </para>
/// </summary>
public class TamNuongRule : ISpiritualRule
{
    public string RuleCode => "TAM_NUONG";

    private static readonly int[] TamNuongDays = { 3, 7, 13, 18, 22, 27 };

    public RuleResult Evaluate(UserProfile profile, LunarDateContext context)
    {
        bool isPassed = !TamNuongDays.Contains(context.LunarDay);

        return new RuleResult
        {
            RuleName = RuleCode,
            IsPassed = isPassed,
            Score    = isPassed ? 0 : -8,
            Message  = isPassed
                ? $"Không phạm Tam Nuông (ngày {context.LunarDay})"
                : $"Phạm Tam Nuông (mùng {context.LunarDay}) — ngày kiêng lửa"
        };
    }
}