using ZenTam.Api.Common.Rules;
using ZenTam.Api.Common.Rules.Models;

namespace ZenTam.Api.Features.EvaluateSpiritualAction.Rules;

/// <summary>
/// <para><b>Xung Tuổi V2</b> — "Age Conflict" taboo (ISpiritualRule).</para>
/// <para>
/// Xung Tuổi là khi Chi của ngày xung với Chi của năm sinh tuổi.
/// Đây là một trong những kiêng kỵ quan trọng nhất trong phong thủy.
/// </para>
/// <para><b>Algorithm:</b></para>
/// <para>
/// 1. Chi Tuổi = ((LunarYOB + 8) mod 12) + 1  (1=Tý, ..., 12=Hợi)<br/>
/// 2. Chi Ngày = ((Jdn + 8) mod 12) + 1       (from RuleContext.Lunar.Jdn)<br/>
/// 3. If CanChiTuoi available from MonthlyEngine path, also check Can<br/>
/// 4. Xung pairs: Tý↔Ngọ, Sửu↔Mùi, Dần↔Thân, Mão↔Dậu, Thìn↔Tuất, Tỵ↔Hợi<br/>
/// 5. Fail (chi xung) → Severity=Hung, IsBlocked=true, ScoreImpact=-15, IsMandatory=true
/// </para>
/// </summary>
public class XungTuoiRuleV2 : ISpiritualRule
{
    public string RuleCode => "XUNG_TUOI";

    private static readonly string[] ChiNames =
        { "", "Tý", "Sửu", "Dần", "Mão", "Thìn", "Tỵ", "Ngọ", "Mùi", "Thân", "Dậu", "Tuất", "Hợi" };

    /// <summary>
    /// Chi xung pairs (opposite in 12-branch cycle, 6 positions apart).
    /// Mapping: Tý↔Ngọ, Sửu↔Mùi, Dần↔Thân, Mão↔Dậu, Thìn↔Tuất, Tỵ↔Hợi
    /// </summary>
    private static readonly Dictionary<int, int> ChiXungPairs = new()
    {
        { 1, 7 },   // Tý ↔ Ngọ
        { 7, 1 },   // Ngọ ↔ Tý
        { 2, 8 },   // Sửu ↔ Mùi
        { 8, 2 },   // Mùi ↔ Sửu
        { 3, 9 },   // Dần ↔ Thân
        { 9, 3 },   // Thân ↔ Dần
        { 4, 10 },  // Mão ↔ Dậu
        { 10, 4 },  // Dậu ↔ Mão
        { 5, 11 },  // Thìn ↔ Tuất
        { 11, 5 },  // Tuất ↔ Thìn
        { 6, 12 },  // Tỵ ↔ Hợi
        { 12, 6 }   // Hợi ↔ Tỵ
    };

    /// <summary>
    /// Can xung pairs (opposite in 10-can cycle, 5 positions apart).
    /// Mapping: Giáp↔Ất, Bính↔Đinh, Mậu↔Kỷ, Canh↔Tân, Nhâm↔Quý
    /// </summary>
    private static readonly (string, string)[] CanXungPairs =
    [
        ("Giáp", "Ất"), ("Bính", "Đinh"), ("Mậu", "Kỷ"), ("Canh", "Tân"), ("Nhâm", "Quý")
    ];

    /// <summary>
    /// Chi from year formula: ((LunarYOB + 8) % 12) + 1
    /// </summary>
    private static int GetChiFromYear(int year) => ((year + 8) % 12) + 1;

    /// <summary>
    /// Chi from JDN formula: ((Jdn + 8) % 12) + 1
    /// </summary>
    private static int GetChiFromJdn(int jdn) => ((jdn + 8) % 12) + 1;

    public RuleEvaluation Evaluate(RuleContext context)
    {
        // Derive Chi from LunarYOB (formula-based)
        int yobChi = GetChiFromYear(context.Profile.LunarYOB);

        // Derive day Chi from JDN (formula-based)
        int dayChi = GetChiFromJdn(context.Lunar.Jdn);

        bool chiXung = ChiXungPairs.TryGetValue(yobChi, out int xungChi) && dayChi == xungChi;

        // If CanChiTuoi is provided (from MonthlyEngine path), also check Can
        bool canXung = false;
        if (context.CanChiTuoi != null && context.CanChiNgay != null)
        {
            canXung = IsCanXung(context.CanChiNgay.Can, context.CanChiTuoi.Can);
        }

        bool isXung = chiXung || canXung;
        bool isDaiHung = chiXung && canXung; // Both chi and can xung = Đại Hung

        return new RuleEvaluation
        {
            RuleCode = RuleCode,
            IsPassed = !isXung,
            ScoreImpact = isXung ? -15 : 0,
            Severity = isDaiHung ? RuleSeverity.DaiHung : (isXung ? RuleSeverity.Hung : RuleSeverity.None),
            IsBlocked = isXung,
            IsMandatory = isXung,
            Message = isXung
                ? $"Phạm Xung Tuổi (ngày {ChiNames[dayChi]} xung tuổi {ChiNames[yobChi]})"
                : $"Không phạm Xung Tuổi (ngày {ChiNames[dayChi]} — tuổi {ChiNames[yobChi]})"
        };
    }

    private static bool IsCanXung(string can1, string can2)
    {
        foreach (var (a, b) in CanXungPairs)
        {
            if ((can1 == a && can2 == b) || (can1 == b && can2 == a))
                return true;
        }
        return false;
    }
}
