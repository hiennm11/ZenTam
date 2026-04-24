using ZenTam.Api.Common.Domain;
using ZenTam.Api.Common.Rules;

namespace ZenTam.Api.Features.EvaluateSpiritualAction.Rules;

/// <summary>
/// <para><b>Tam Tai (Ba Năm Tam Tai)</b> — "Three Disasters" taboo.</para>
/// <para>
/// Theo quan niệm dân gian Việt Nam và Trung Hoa, mỗi người trải qua một chu kỳ
/// "Tam Tai" kéo dài ba năm liên tiếp, trong đó dễ gặp tai hoạ, thất bại và bệnh tật.
/// Nhóm Tam Tai được xác định dựa trên địa chi năm sinh (Can Chi âm lịch).
/// Trong ba năm Tam Tai, người ta thường tránh khởi công xây nhà, cưới hỏi hay
/// làm những việc lớn.
/// </para>
/// <para><b>Algorithm (English):</b></para>
/// <para>
/// 1. Compute Chi (Earthly Branch index 1–12) for any year: chi = ((year + 8) mod 12) + 1<br/>
///    Mapping: 1→Tý, 2→Sửu, 3→Dần, 4→Mão, 5→Thìn, 6→Tỵ,
///             7→Ngọ, 8→Mùi, 9→Thân, 10→Dậu, 11→Tuất, 12→Hợi<br/>
/// 2. yobChi     = chi(LunarYOB)<br/>
/// 3. targetChi  = chi(TargetYear)<br/>
/// 4. Look up the YobChiGroup that contains yobChi to get its ForbiddenChiGroup.<br/>
/// 5. Fail when targetChi ∈ ForbiddenChiGroup; pass otherwise.<br/>
/// 6. Score on fail: −10.
/// </para>
/// </summary>
public class TamTaiRule : ISpiritualRule
{
    public string RuleCode => "Rule_TamTai";

    // Index 0 unused; Chi values are 1-based.
    private static readonly string[] ChiNames =
        { "", "Tý", "Sửu", "Dần", "Mão", "Thìn", "Tỵ", "Ngọ", "Mùi", "Thân", "Dậu", "Tuất", "Hợi" };

    private static readonly List<(int[] YobChiGroup, int[] ForbiddenChiGroup)> TamTaiMap = new()
    {
        (new[] { 3,  7, 11 }, new[] { 9, 10, 11 }),   // Dần/Ngọ/Tuất → Thân/Dậu/Tuất
        (new[] { 12, 4,  8 }, new[] { 6,  7,  8 }),   // Hợi/Mão/Mùi  → Tỵ/Ngọ/Mùi
        (new[] { 9,  1,  5 }, new[] { 3,  4,  5 }),   // Thân/Tý/Thìn  → Dần/Mão/Thìn
        (new[] { 6, 10,  2 }, new[] { 12, 1,  2 })    // Tỵ/Dậu/Sửu   → Hợi/Tý/Sửu
    };

    private static int GetChi(int year) => ((year + 8) % 12) + 1;

    public RuleResult Evaluate(UserProfile profile, LunarDateContext context)
    {
        int yobChi    = GetChi(profile.LunarYOB);
        int targetChi = GetChi(profile.TargetYear);

        var entry = TamTaiMap.FirstOrDefault(e => e.YobChiGroup.Contains(yobChi));
        bool isPassed = entry.ForbiddenChiGroup == null || !entry.ForbiddenChiGroup.Contains(targetChi);

        if (isPassed)
        {
            return new RuleResult
            {
                RuleName = RuleCode,
                IsPassed = true,
                Score    = 0,
                Message  = $"Không phạm Tam Tai (năm {ChiNames[targetChi]})"
            };
        }

        string forbiddenChiNamesJoined = string.Join(", ",
            entry.ForbiddenChiGroup!.Select(c => ChiNames[c]));

        return new RuleResult
        {
            RuleName = RuleCode,
            IsPassed = false,
            Score    = -10,
            Message  = $"Phạm Tam Tai (năm {ChiNames[targetChi]} — tuổi {ChiNames[yobChi]} kỵ {forbiddenChiNamesJoined})"
        };
    }
}
