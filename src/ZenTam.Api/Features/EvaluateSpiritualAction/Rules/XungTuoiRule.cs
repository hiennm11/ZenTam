using ZenTam.Api.Common.Domain;
using ZenTam.Api.Common.Rules;

namespace ZenTam.Api.Features.EvaluateSpiritualAction.Rules;

/// <summary>
/// <para><b>Xung Tuổi</b> — "Age Conflict" taboo.</para>
/// <para>
/// Xung Tuổi là khi Chi của ngày xung với Chi của năm sinh tuổi.
/// Đây là một trong những kiêng kỵ quan trọng nhất trong phong thủy.
/// </para>
/// <para><b>Algorithm:</b></para>
/// <para>
/// 1. Chi Tuổi = ((LunarYOB + 8) mod 12) + 1  (1=Tý, ..., 12=Hợi)<br/>
/// 2. Chi Ngày = ((Jdn + 8) mod 12) + 1       (from LunarDateContext.Jdn)<br/>
/// 3. Xung pairs: Tý↔Ngọ, Sửu↔Mùi, Dần↔Thân, Mão↔Dậu, Thìn↔Tuất, Tỉ↔Hợi<br/>
/// 4. targetChi == xung(yobChi) → FAIL, Score −15, IsMandatory = true
/// </para>
/// </summary>
public class XungTuoiRule : ISpiritualRule
{
    public string RuleCode => "XUNG_TUOI";

    private static readonly string[] ChiNames =
        { "", "Tý", "Sửu", "Dần", "Mão", "Thìn", "Tỵ", "Ngọ", "Mùi", "Thân", "Dậu", "Tuất", "Hợi" };

    /// <summary>
    /// Chi xung pairs (opposite in 12-branch cycle, 6 positions apart).
    /// Mapping: Tý↔Ngọ, Sửu↔Mùi, Dần↔Thân, Mão↔Dậu, Thìn↔Tuất, Tỵ↔Hợi
    /// </summary>
    private static readonly Dictionary<int, int> XungPairs = new()
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
    /// Get Chi from year.
    /// </summary>
    private static int GetChiFromYear(int year) => ((year + 8) % 12) + 1;

    /// <summary>
    /// Get Chi from Julian Day Number (for day).
    /// </summary>
    private static int GetChiFromJdn(int jdn) => ((jdn + 8) % 12) + 1;

    public RuleResult Evaluate(UserProfile profile, LunarDateContext context)
    {
        int yobChi = GetChiFromYear(profile.LunarYOB);
        int dayChi = GetChiFromJdn(context.Jdn);

        bool isXung = XungPairs.TryGetValue(yobChi, out int xungChi) && dayChi == xungChi;

        return new RuleResult
        {
            RuleName = RuleCode,
            IsPassed = !isXung,
            IsMandatory = true,
            Score    = isXung ? -15 : 0,
            Message  = isXung
                ? $"Phạm Xung Tuổi (ngày {ChiNames[dayChi]} xung tuổi {ChiNames[yobChi]})"
                : $"Không phạm Xung Tuổi (ngày {ChiNames[dayChi]} — tuổi {ChiNames[yobChi]})"
        };
    }
}