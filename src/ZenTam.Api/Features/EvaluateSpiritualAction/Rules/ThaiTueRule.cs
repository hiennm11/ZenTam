using ZenTam.Api.Common.Domain;
using ZenTam.Api.Common.Rules;

namespace ZenTam.Api.Features.EvaluateSpiritualAction.Rules;

/// <summary>
/// <para><b>Thái Tuế (Phạm Thái Tuế)</b> — "Grand Duke Jupiter" taboo.</para>
/// <para>
/// Thái Tuế là vị thần cai quản năm âm lịch. Trong dân gian Việt Nam và Trung Hoa,
/// làm việc lớn vào năm Thái Tuế chiếu mệnh hay xung mệnh là điều tối kỵ.
/// </para>
/// <para><b>Algorithm:</b></para>
/// <para>
/// 1. Compute Chi (Earthly Branch 1–12): chi = ((year + 8) mod 12) + 1<br/>
///    1→Tý, 2→Sửu, 3→Dần, 4→Mão, 5→Thìn, 6→Tỵ, 7→Ngọ, 8→Mùi, 9→Thân, 10→Dậu, 11→Tuất, 12→Hợi<br/>
/// 2. yobChi    = chi(LunarYOB)<br/>
/// 3. targetChi = chi(TargetYear)<br/>
/// 4. Phạm Thái Tuế  : targetChi == yobChi         → Score −10 (IsMandatory per mapping)<br/>
/// 5. Phạm Xung Thái Tuế: targetChi == xung(yobChi) → Score −5<br/>
///    where xung(chi) = ((chi + 5) mod 12) + 1  (the branch exactly 6 positions away)<br/>
/// 6. Otherwise: pass, Score 0.
/// </para>
/// </summary>
public class ThaiTueRule : ISpiritualRule
{
    public string RuleCode => "ThaiTue";

    private static readonly string[] ChiNames =
        { "", "Tý", "Sửu", "Dần", "Mão", "Thìn", "Tỵ", "Ngọ", "Mùi", "Thân", "Dậu", "Tuất", "Hợi" };

    private static int GetChi(int year) => ((year + 8) % 12) + 1;

    /// <summary>Returns the branch directly opposite (6 steps away) in the 12-branch cycle.</summary>
    private static int GetXung(int chi) => ((chi + 5) % 12) + 1;

    public RuleResult Evaluate(UserProfile profile, LunarDateContext context)
    {
        int yobChi    = GetChi(profile.LunarYOB);
        int targetChi = GetChi(profile.TargetYear);
        int xungChi   = GetXung(yobChi);

        if (targetChi == yobChi)
        {
            return new RuleResult
            {
                RuleName = RuleCode,
                IsPassed = false,
                Score    = -10,
                Message  = $"Phạm Thái Tuế (năm {ChiNames[targetChi]} — tuổi {ChiNames[yobChi]} gặp Thái Tuế chiếu mệnh)"
            };
        }

        if (targetChi == xungChi)
        {
            return new RuleResult
            {
                RuleName = RuleCode,
                IsPassed = false,
                Score    = -5,
                Message  = $"Phạm Xung Thái Tuế (năm {ChiNames[targetChi]} xung với tuổi {ChiNames[yobChi]})"
            };
        }

        return new RuleResult
        {
            RuleName = RuleCode,
            IsPassed = true,
            Score    = 0,
            Message  = $"Không phạm Thái Tuế (năm {ChiNames[targetChi]} — tuổi {ChiNames[yobChi]})"
        };
    }
}
