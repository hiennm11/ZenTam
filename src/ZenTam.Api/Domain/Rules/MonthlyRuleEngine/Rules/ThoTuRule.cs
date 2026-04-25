namespace ZenTam.Api.Domain.Rules.MonthlyRuleEngine.Rules;

using Models;

/// <summary>
/// Thọ Tử Rule - Bảng theo tháng âm lịch và chi ngày.
/// Tháng: 1=Mùi, 2=Thân, 3=Dậu, 4=Tuất, 5=Hợi, 6=Tý, 7=Sửu, 8=Mão, 9=Dần, 10=Thìn, 11=Tỵ, 12=Ngọ
/// Severity: 4 (Đại Hung)
/// </summary>
public sealed class ThoTuRule : IMonthlyRule
{
    private static readonly string[] ThoTuChiByMonth =
    [
        "Mùi",  // Month 1
        "Thân", // Month 2
        "Dậu",  // Month 3
        "Tuất", // Month 4
        "Hợi",  // Month 5
        "Tý",   // Month 6
        "Sửu",  // Month 7
        "Mão",  // Month 8
        "Dần",  // Month 9
        "Thìn", // Month 10
        "Tỵ",   // Month 11
        "Ngọ"   // Month 12
    ];

    public string RuleCode => "THO_TU";

    public Violation? Evaluate(int lunarDay, int lunarMonth, DateTime? solarDate, string chiNgay, Common.CanChi.Models.CanChiYear? canChiTuoi)
    {
        var badChi = ThoTuChiByMonth[(lunarMonth - 1) % 12];

        if (chiNgay != badChi)
            return null;

        return new Violation
        {
            RuleCode = RuleCode,
            Severity = 4,
            Message = $"Ngày {chiNgay} là Thọ Tử tháng {lunarMonth} - đại kỵ",
            IsBlocked = true
        };
    }
}