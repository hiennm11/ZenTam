namespace ZenTam.Api.Domain.Rules.MonthlyRuleEngine;

using Common.Rules;
using Common.Rules.Models;
using Common.Lunar;
using Common.CanChi;
using Common.Domain;
using Models;
using ZenTam.Api.Common.CanChi.Models;
using ZenTam.Api.Domain.Rules.MonthlyRuleEngine.Rules;

public class MonthlyRuleEngine : IMonthlyRuleEngine
{
    private readonly IEnumerable<ISpiritualRule> _rules;
    private readonly ILunarCalculatorService? _lunarCalculator;
    private readonly ICanChiCalculator? _canChiCalculator;

    // Constructor for pre-migrated ISpiritualRule instances
    public MonthlyRuleEngine(IEnumerable<ISpiritualRule> rules)
    {
        _rules = rules;
        _lunarCalculator = null;
        _canChiCalculator = null;
    }

    // Constructor for legacy IMonthlyRule instances (wraps them via MonthlyRuleAdapter)
    // NOTE: MonthlyRuleAdapter and IMonthlyRule have been removed as part of Option C.
    // This constructor is kept as a stub to prevent build breakage from any residual callers.
    public MonthlyRuleEngine(
        IEnumerable<object> legacyRules,
        ILunarCalculatorService lunarCalculator,
        ICanChiCalculator canChiCalculator)
    {
        throw new NotSupportedException(
            "IMonthlyRule-based MonthlyRuleEngine constructor is no longer supported. " +
            "Use the ISpiritualRule-based constructor instead.");
    }

    public MonthlyEvaluationResult Evaluate(
        DateTime solarDate,
        int lunarDay,
        int lunarMonth,
        bool isLeap,
        CanChiDay canChiNgay,
        CanChiYear canChiTuoi)
    {
        // Compute LunarYear and Jdn from solarDate using injected services
        int lunarYear = 0;
        int jdn = 0;
        int solarMonth = solarDate.Month;

        if (_lunarCalculator != null && _canChiCalculator != null)
        {
            var lunarContext = _lunarCalculator.Convert(solarDate);
            lunarYear = lunarContext.LunarYear;
            jdn = lunarContext.Jdn;
            solarMonth = lunarContext.SolarMonth;
        }
        else if (_canChiCalculator != null)
        {
            jdn = _canChiCalculator.GetJulianDayNumber(solarDate);
            // Fallback: approximate lunar year from solar date
            lunarYear = solarDate.Year - 1; // Placeholder, services should compute this
        }

        // Build RuleContext from parameters
        var context = new RuleContext
        {
            Profile = new UserProfile { Gender = Gender.Male, LunarYOB = 0, TargetYear = solarDate.Year },
            Lunar = new LunarDateContext
            {
                LunarYear = lunarYear,
                LunarMonth = lunarMonth,
                LunarDay = lunarDay,
                IsLeap = isLeap,
                Jdn = jdn,
                SolarMonth = solarMonth
            },
            CanChiNgay = canChiNgay,
            CanChiTuoi = canChiTuoi
        };

        var violations = new List<Violation>();
        var dayLevel = DayLevelCalculator.DetermineDayLevel(lunarDay, lunarMonth);

        foreach (var rule in _rules)
        {
            var result = rule.Evaluate(context);
            if (!result.IsPassed)
            {
                violations.Add(new Violation
                {
                    RuleCode = result.RuleCode,
                    Severity = (int)result.Severity,
                    Message = result.Message,
                    IsBlocked = result.IsBlocked
                });
            }
        }

        int maxSeverity = violations.Count > 0 ? violations.Max(v => v.Severity) : 0;
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