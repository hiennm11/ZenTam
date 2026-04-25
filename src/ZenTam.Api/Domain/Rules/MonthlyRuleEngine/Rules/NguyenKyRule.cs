namespace ZenTam.Api.Domain.Rules.MonthlyRuleEngine.Rules;

using Models;

/// <summary>
/// Nguyệt Kỵ Rule - Ngày 5, 14, 23 Âm lịch là đại kỵ mọi việc lớn.
/// Severity: 4 (Đại Hung)
/// </summary>
public sealed class NguyenKyRule : IMonthlyRule
{
    private static readonly HashSet<int> NguyenKyDays = [5, 14, 23];

    public string RuleCode => "NGUYET_KY";

    public Violation? Evaluate(int lunarDay, int lunarMonth, DateTime? solarDate, string chiNgay, Common.CanChi.Models.CanChiYear? canChiTuoi)
    {
        if (!NguyenKyDays.Contains(lunarDay))
            return null;

        return new Violation
        {
            RuleCode = RuleCode,
            Severity = 4,
            Message = $"Ngày {lunarDay} Âm là Nguyệt Kỵ - đại kỵ mọi việc lớn",
            IsBlocked = true
        };
    }
}