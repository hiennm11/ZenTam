using ZenTam.Api.Common.Domain;
using ZenTam.Api.Common.Rules;

namespace ZenTam.Api.Features.EvaluateSpiritualAction.Rules;

/// <summary>
/// <para><b>Hoàng Ốc (Tuổi Hoàng Ốc)</b> — "Yellow House" taboo.</para>
/// <para>
/// Hoàng Ốc là một kiêng kỵ dân gian áp dụng khi xây nhà. Người rơi vào cung "Lục Hoàng Ốc",
/// "Tam Địa Sát" hoặc "Ngũ Thọ Tử" sẽ gặp nhiều rắc rối sau khi chuyển vào nhà mới.
/// Sáu cung xoay vòng theo từng năm tuổi âm.
/// </para>
/// <para><b>Algorithm (English):</b></para>
/// <para>
/// 1. LunarAge = TargetYear − LunarYOB + 1<br/>
/// 2. index = (LunarAge − 1) mod 6   (0-based, cycles through 6 cung)<br/>
/// 3. Fail when index ∈ { 2, 4, 5 }; pass otherwise.<br/>
/// 4. Score on fail: −5.
/// </para>
/// </summary>
public class HoangOcRule : ISpiritualRule
{
    public string RuleCode => "HoangOc";

    private static readonly string[] CungNames =
    {
        "Nhất Cát",       // 0 — pass
        "Nhị Nghi",       // 1 — pass
        "Tam Địa Sát",    // 2 — FAIL
        "Tứ Tấn Tài",     // 3 — pass
        "Ngũ Thọ Tử",     // 4 — FAIL
        "Lục Hoàng Ốc"    // 5 — FAIL
    };

    private static readonly int[] FailIndexes = { 2, 4, 5 };

    public RuleResult Evaluate(UserProfile profile, LunarDateContext context)
    {
        int lunarAge = profile.TargetYear - profile.LunarYOB + 1;
        int index    = (lunarAge - 1) % 6;
        bool isPassed = !FailIndexes.Contains(index);

        return new RuleResult
        {
            RuleName  = RuleCode,
            IsPassed  = isPassed,
            Score     = isPassed ? 0 : -5,
            Message   = isPassed
                ? $"Không phạm Hoàng Ốc — cung {CungNames[index]}"
                : $"Phạm {CungNames[index]} — Hoàng Ốc"
        };
    }
}
