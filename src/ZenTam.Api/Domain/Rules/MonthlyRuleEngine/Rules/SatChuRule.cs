namespace ZenTam.Api.Domain.Rules.MonthlyRuleEngine.Rules;

using Models;

/// <summary>
/// Sát Chủ Rule - Bảng theo tháng âm lịch và chi ngày.
/// Severity: 4 (Đại Hung)
/// </summary>
public sealed class SatChuRule : IMonthlyRule
{
    // Month index 0-11, [month, 0] = Dương chi, [month, 1] = Âm chi
    private static readonly string[,] SatChuTable = new string[,]
    {
        // Month 1-6
        {"Mão", "Dậu"}, {"Tỵ", "Hợi"}, {"Thìn", "Tuất"}, 
        {"Ngọ", "Mùi"}, {"Thân", "Sửu"}, {"Dậu", "Tý"},
        // Month 7-12 (repeat pattern)
        {"Mão", "Dậu"}, {"Tỵ", "Hợi"}, {"Thìn", "Tuất"}, 
        {"Ngọ", "Mùi"}, {"Thân", "Sửu"}, {"Dậu", "Tý"}
    };

    public string RuleCode => "SAT_CHU";

    public Violation? Evaluate(int lunarDay, int lunarMonth, DateTime? solarDate, string chiNgay, Common.CanChi.Models.CanChiYear? canChiTuoi)
    {
        var monthIndex = (lunarMonth - 1) % 12;
        var duongChi = SatChuTable[monthIndex, 0];
        var amChi = SatChuTable[monthIndex, 1];

        if (chiNgay != duongChi && chiNgay != amChi)
            return null;

        return new Violation
        {
            RuleCode = RuleCode,
            Severity = 4,
            Message = $"Ngày {chiNgay} là Sát Chủ tháng {lunarMonth} - đại kỵ",
            IsBlocked = true
        };
    }
}