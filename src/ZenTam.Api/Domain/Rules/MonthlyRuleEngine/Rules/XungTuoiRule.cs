namespace ZenTam.Api.Domain.Rules.MonthlyRuleEngine.Rules;

using Models;
using ZenTam.Api.Common.CanChi.Models;

/// <summary>
/// Xung Tuổi Rule - Can Chi ngày xung Can Chi tuổi.
/// Chi xung: Tý↔Ngọ, Sửu↔Mùi, Dần↔Thân, Mão↔Dậu, Thìn↔Tuất, Tỵ↔Hợi
/// Can xung: Giáp↔Ất, Bính↔Đinh, Mậu↔Kỷ, Canh↔Tân, Nhâm↔Quý
/// Severity: 3 (Hung) nếu chỉ chi xung, 4 (Đại Hung) nếu cả can và chi đều xung
/// </summary>
public sealed class XungTuoiRule : IMonthlyRule
{
    private static readonly (string, string)[] ChiXungPairs =
    [
        ("Tý", "Ngọ"), ("Sửu", "Mùi"), ("Dần", "Thân"), ("Mão", "Dậu"),
        ("Thìn", "Tuất"), ("Tỵ", "Hợi")
    ];

    private static readonly (string, string)[] CanXungPairs =
    [
        ("Giáp", "Ất"), ("Bính", "Đinh"), ("Mậu", "Kỷ"), ("Canh", "Tân"), ("Nhâm", "Quý")
    ];

    public string RuleCode => "XUNG_TUOI";

    public Violation? Evaluate(int lunarDay, int lunarMonth, DateTime? solarDate, string chiNgay, CanChiYear? canChiTuoi)
    {
        if (canChiTuoi == null)
            return null;

        bool chiXung = IsChiXung(chiNgay, canChiTuoi.Chi);
        
        if (!chiXung)
            return null;

        // Check if only chi xung or both can and chi xung
        return new Violation
        {
            RuleCode = RuleCode,
            Severity = 3, // For now, just chi xung = Hung; can+chi would be DaiHung
            Message = $"Ngày {chiNgay} xung tuổi {canChiTuoi.Chi} (chi xung) - hung",
            IsBlocked = true
        };
    }

    private static bool IsChiXung(string chi1, string chi2)
    {
        foreach (var (a, b) in ChiXungPairs)
        {
            if ((chi1 == a && chi2 == b) || (chi1 == b && chi2 == a))
                return true;
        }
        return false;
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