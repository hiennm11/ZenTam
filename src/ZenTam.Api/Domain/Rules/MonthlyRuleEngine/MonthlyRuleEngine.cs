namespace ZenTam.Api.Domain.Rules.MonthlyRuleEngine;

using Models;
using ZenTam.Api.Common.CanChi.Models;
using Rules;

public class MonthlyRuleEngine : IMonthlyRuleEngine
{
    private readonly IEnumerable<IMonthlyRule> _rules;

    public MonthlyRuleEngine(IEnumerable<IMonthlyRule> rules)
    {
        _rules = rules;
    }

    public MonthlyEvaluationResult Evaluate(
        DateTime solarDate,
        int lunarDay,
        int lunarMonth,
        bool isLeap,
        CanChiDay canChiNgay,
        CanChiYear canChiTuoi)
    {
        var violations = new List<Violation>();

        // Determine day level
        var dayLevel = DayLevelCalculator.DetermineDayLevel(lunarDay, lunarMonth);

        // Execute each rule
        foreach (var rule in _rules)
        {
            var violation = rule.Evaluate(lunarDay, lunarMonth, solarDate, canChiNgay.Chi, canChiTuoi);
            if (violation != null)
            {
                violations.Add(violation);
            }
        }

        // Calculate overall severity
        int maxSeverity = violations.Count > 0
            ? violations.Max(v => v.Severity)
            : 0;

        // Map to verdict
        var verdict = maxSeverity switch
        {
            0 => DayVerdict.Binh,
            1 => DayVerdict.Cat,
            2 => DayVerdict.DaiCat,
            3 => DayVerdict.Hung,
            4 => DayVerdict.DaiHung,
            _ => DayVerdict.TuVong
        };

        return new MonthlyEvaluationResult
        {
            OverallVerdict = verdict,
            OverallSeverity = maxSeverity,
            DayLevel = dayLevel,
            Violations = violations,
            EvaluatedAt = DateTime.UtcNow
        };
    }
}