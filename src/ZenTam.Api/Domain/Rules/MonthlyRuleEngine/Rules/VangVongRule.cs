namespace ZenTam.Api.Domain.Rules.MonthlyRuleEngine.Rules;

using Models;

/// <summary>
/// Vãng Vong Rule - Bảng theo tháng âm lịch và chi ngày.
/// Tháng: 1=Tý, 2=Sửu, 3=Dần, 4=Mão, 5=Thìn, 6=Tỵ, 7=Ngọ, 8=Mùi, 9=Thân, 10=Dậu, 11=Tuất, 12=Hợi
/// Severity: 3 (Hung)
/// </summary>
public sealed class VangVongRule : IMonthlyRule
{
    private static readonly string[] VangVongChiByMonth =
    [
        "Tý",   // Month 1
        "Sửu",  // Month 2
        "Dần",  // Month 3
        "Mão",  // Month 4
        "Thìn", // Month 5
        "Tỵ",   // Month 6
        "Ngọ",  // Month 7
        "Mùi",  // Month 8
        "Thân", // Month 9
        "Dậu",  // Month 10
        "Tuất", // Month 11
        "Hợi"   // Month 12
    ];

    public string RuleCode => "VANG_VONG";

    public Violation? Evaluate(int lunarDay, int lunarMonth, DateTime? solarDate, string chiNgay, Common.CanChi.Models.CanChiYear? canChiTuoi)
    {
        var badChi = VangVongChiByMonth[(lunarMonth - 1) % 12];

        if (chiNgay != badChi)
            return null;

        return new Violation
        {
            RuleCode = RuleCode,
            Severity = 3,
            Message = $"Ngày {chiNgay} là Vãng Vong tháng {lunarMonth} - hung",
            IsBlocked = false
        };
    }
}