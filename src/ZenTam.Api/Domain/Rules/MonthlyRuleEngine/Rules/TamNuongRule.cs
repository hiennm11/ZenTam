namespace ZenTam.Api.Domain.Rules.MonthlyRuleEngine.Rules;

using Models;

/// <summary>
/// Tam Nương Rule - Ngày 3, 7, 13, 18, 22, 27 Âm lịch kỵ việc cưới hỏi, xuất hành.
/// Severity: 3 (Hung)
/// </summary>
public sealed class TamNuongRule : IMonthlyRule
{
    private static readonly HashSet<int> TamNuongDays = [3, 7, 13, 18, 22, 27];

    public string RuleCode => "TAM_NUONG";

    public Violation? Evaluate(int lunarDay, int lunarMonth, DateTime? solarDate, string chiNgay, Common.CanChi.Models.CanChiYear? canChiTuoi)
    {
        if (!TamNuongDays.Contains(lunarDay))
            return null;

        return new Violation
        {
            RuleCode = RuleCode,
            Severity = 3,
            Message = $"Ngày {lunarDay} Âm là Tam Nương - kỵ việc cưới hỏi, xuất hành",
            IsBlocked = true
        };
    }
}