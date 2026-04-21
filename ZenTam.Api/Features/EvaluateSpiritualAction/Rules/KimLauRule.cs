using ZenTam.Api.Common.Domain;
using ZenTam.Api.Common.Rules;

namespace ZenTam.Api.Features.EvaluateSpiritualAction.Rules;

/// <summary>
/// <para><b>Kim Lâu (Tuổi Kim Lâu)</b> — "Golden Prison" taboo.</para>
/// <para>
/// Trong tín ngưỡng dân gian Việt Nam, Kim Lâu là một trong những điều kiêng kỵ quan trọng
/// khi xây dựng nhà cửa. Người đàn ông rơi vào năm Kim Lâu không được động thổ, xây nhà,
/// kẻo gặp tai hoạ liên quan đến bản thân (Thân), vợ (Thê), con cái (Tử) hoặc gia súc (Lục Súc).
/// </para>
/// <para><b>Algorithm (English):</b></para>
/// <para>
/// 1. LunarAge = TargetYear − LunarYOB + 1<br/>
/// 2. remainder = LunarAge mod 9<br/>
/// 3. Fail when remainder ∈ { 1, 3, 6, 8 }; pass otherwise.<br/>
/// 4. Score on fail: −5.
/// </para>
/// </summary>
public class KimLauRule : ISpiritualRule
{
    public string RuleCode => "Rule_KimLau";

    private static readonly int[] FailRemainders = { 1, 3, 6, 8 };

    private static readonly Dictionary<int, string> FailMessages = new()
    {
        { 1, "Kim Lâu Thân"     },
        { 3, "Kim Lâu Thê"      },
        { 6, "Kim Lâu Tử"       },
        { 8, "Kim Lâu Lục Súc"  }
    };

    public RuleResult Evaluate(UserProfile profile, LunarDateContext context)
    {
        int lunarAge  = profile.TargetYear - profile.LunarYOB + 1;
        int remainder = lunarAge % 9;
        bool isPassed = !FailRemainders.Contains(remainder);

        return new RuleResult
        {
            RuleName  = RuleCode,
            IsPassed  = isPassed,
            Score     = isPassed ? 0 : -5,
            Message   = isPassed
                ? $"Không phạm Kim Lâu (số dư {remainder})"
                : $"Phạm {FailMessages[remainder]} — Kim Lâu (số dư {remainder})"
        };
    }
}
