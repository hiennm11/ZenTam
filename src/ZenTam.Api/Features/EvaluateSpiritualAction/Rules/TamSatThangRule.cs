using ZenTam.Api.Common.CanChi;
using ZenTam.Api.Common.CanChi.Models;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Common.Rules;

namespace ZenTam.Api.Features.EvaluateSpiritualAction.Rules;

/// <summary>
/// <para><b>Tam Sát Tháng</b> — "Three Swords of the Month" taboo.</para>
/// <para>
/// Tam Sát Tháng là kiêng kỵ liên quan đến tháng âm lịch.
/// Dựa trên Chi của tháng để xác định các ngày Kiếp Sát, Tai Sát, Niên Sát.
/// </para>
/// <para><b>Algorithm:</b></para>
/// <para>
/// 1. Get CanChi of LunarMonth using CanChiCalculator<br/>
/// 2. Map month Chi to SatDays configuration:<br/>
///    - Tý, Thìn, Ngọ, Tuất → Kiếp Sát: 1, 3, 5; Tai Sát: 2, 4, 6<br/>
///    - Sửu, Mão, Mùi, Dậu → Kiếp Sát: 2, 4, 6; Tai Sát: 1, 3, 5<br/>
///    - Dần, Tỵ, Thân, Hợi → Kiếp Sát: 3, 5, 7; Tai Sát: 2, 4, 6<br/>
/// 3. Fail when LunarDay ∈ SatDays
/// 4. Score on fail: −12, IsMandatory = true
/// </para>
/// </summary>
public class TamSatThangRule : ISpiritualRule
{
    public string RuleCode => "TAM_SAT_THANG";

    private readonly ICanChiCalculator _canChi;

    /// <summary>
    /// Chi index to (KiepSat, TaiSat) mapping.
    /// Chi 1=Tý, 2=Sửu, 3=Dần, 4=Mão, 5=Thìn, 6=Tỵ, 7=Ngọ, 8=Mùi, 9=Thân, 10=Dậu, 11=Tuất, 12=Hợi
    /// </summary>
    private static readonly Dictionary<int, (int[] KiepSat, int[] TaiSat)> ChiSatMapping = new()
    {
        // Tý, Thìn, Ngọ, Tuất group
        { 1,  (new[] { 1, 3, 5 }, new[] { 2, 4, 6 }) },  // Tý
        { 5,  (new[] { 1, 3, 5 }, new[] { 2, 4, 6 }) },  // Thìn
        { 7,  (new[] { 1, 3, 5 }, new[] { 2, 4, 6 }) },  // Ngọ
        { 11, (new[] { 1, 3, 5 }, new[] { 2, 4, 6 }) },  // Tuất

        // Sửu, Mão, Mùi, Dậu group
        { 2,  (new[] { 2, 4, 6 }, new[] { 1, 3, 5 }) },  // Sửu
        { 4,  (new[] { 2, 4, 6 }, new[] { 1, 3, 5 }) },  // Mão
        { 8,  (new[] { 2, 4, 6 }, new[] { 1, 3, 5 }) },  // Mùi
        { 10, (new[] { 2, 4, 6 }, new[] { 1, 3, 5 }) },  // Dậu

        // Dần, Tỵ, Thân, Hợi group
        { 3,  (new[] { 3, 5, 7 }, new[] { 2, 4, 6 }) },  // Dần
        { 6,  (new[] { 3, 5, 7 }, new[] { 2, 4, 6 }) },  // Tỵ
        { 9,  (new[] { 3, 5, 7 }, new[] { 2, 4, 6 }) },  // Thân
        { 12, (new[] { 3, 5, 7 }, new[] { 2, 4, 6 }) }   // Hợi
    };

    public TamSatThangRule(ICanChiCalculator canChi)
    {
        _canChi = canChi;
    }

    private static readonly string[] ChiNames =
        { "", "Tý", "Sửu", "Dần", "Mão", "Thìn", "Tỵ", "Ngọ", "Mùi", "Thân", "Dậu", "Tuất", "Hợi" };

    public RuleResult Evaluate(UserProfile profile, LunarDateContext context)
    {
        // Get CanChi of the month using calculator
        var canChiThang = _canChi.GetCanChiThang(context.LunarYear, context.LunarMonth, context.IsLeap);

        // Month Chi is computed directly from lunarMonth:
        // Month 1=Tý(1), 2=Sửu(2), ..., 12=Hợi(12)
        int chi = ((context.LunarMonth - 1) % 12) + 1;

        if (!ChiSatMapping.TryGetValue(chi, out var satConfig))
        {
            // Unknown chi - should not happen, but default to pass
            return new RuleResult
            {
                RuleName = RuleCode,
                IsPassed = true,
                IsMandatory = true,
                Score = 0,
                Message = $"Không xác định được Tam Sát Tháng (tháng {context.LunarMonth} {canChiThang.Chi})"
            };
        }

        // Combine Kiếp Sát and Tai Sát days
        var allSatDays = satConfig.KiepSat.Concat(satConfig.TaiSat).Distinct().ToArray();
        bool isSatDay = allSatDays.Contains(context.LunarDay);

        return new RuleResult
        {
            RuleName = RuleCode,
            IsPassed = !isSatDay,
            IsMandatory = true,
            Score = isSatDay ? -12 : 0,
            Message = isSatDay
                ? $"Phạm Tam Sát Tháng (ngày {context.LunarDay} — tháng {context.LunarMonth} {canChiThang.Chi})"
                : $"Không phạm Tam Sát Tháng (ngày {context.LunarDay} — tháng {context.LunarMonth} {canChiThang.Chi})"
        };
    }
}